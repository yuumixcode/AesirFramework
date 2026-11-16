#!/usr/bin/env python3
"""
Convert a GitHub repository or subfolder into a Codely CLI skill or extension.

Auto-detects type:
  - Single MD file only → Skill (installed to skills/)
  - Complex content → Extension (installed to extensions/)

Usage:
    python convert_from_github.py <github_url> [--scope project|global]
    python convert_from_github.py <github_url> --as-extension
    python convert_from_github.py <github_url> --as-skill
    
Example:
    python convert_from_github.py https://github.com/anthropics/skills/tree/main/skills/algorithmic-art
    python convert_from_github.py https://github.com/user/repo --as-extension
"""

import argparse
import json
import os
import re
import shutil
import tempfile
import urllib.request
import zipfile
from pathlib import Path


def parse_github_url(url: str) -> dict:
    """Parse GitHub URL to extract owner, repo, branch, and path."""
    patterns = [
        r'https://github\.com/([^/]+)/([^/]+)/tree/([^/]+)/(.+)',
        r'https://github\.com/([^/]+)/([^/]+)/?$',
    ]
    
    for i, pattern in enumerate(patterns):
        match = re.match(pattern, url)
        if match:
            if i == 0:
                return {
                    'owner': match.group(1),
                    'repo': match.group(2),
                    'branch': match.group(3),
                    'path': match.group(4).rstrip('/'),
                }
            else:
                return {
                    'owner': match.group(1),
                    'repo': match.group(2),
                    'branch': 'main',
                    'path': '',
                }
    
    raise ValueError(f"Invalid GitHub URL format: {url}")


def download_github_repo(owner: str, repo: str, branch: str, temp_dir: str) -> str:
    """Download and extract a GitHub repository."""
    archive_url = f"https://github.com/{owner}/{repo}/archive/refs/heads/{branch}.zip"
    archive_path = os.path.join(temp_dir, f"{repo}.zip")
    
    print(f"Downloading: {archive_url}")
    urllib.request.urlretrieve(archive_url, archive_path)
    
    extract_path = os.path.join(temp_dir, "extracted")
    with zipfile.ZipFile(archive_path, 'r') as zip_ref:
        zip_ref.extractall(extract_path)
    
    extracted_folder = os.path.join(extract_path, f"{repo}-{branch}")
    return extracted_folder


def to_kebab_case(name: str) -> str:
    """Convert a string to kebab-case."""
    name = re.sub(r'[\s_]+', '-', name)
    name = re.sub(r'[^a-zA-Z0-9-]', '', name)
    name = name.lower()
    name = name.strip('-')
    return name


def extract_description_from_readme(readme_path: str) -> str:
    """Extract a description from README.md."""
    if not os.path.exists(readme_path):
        return ""
    
    with open(readme_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    lines = content.split('\n')
    in_first_paragraph = False
    paragraph_lines = []
    
    for line in lines:
        stripped = line.strip()
        if stripped.startswith('#'):
            if in_first_paragraph:
                break
            continue
        if stripped == '':
            if in_first_paragraph:
                break
            continue
        if not in_first_paragraph:
            in_first_paragraph = True
        paragraph_lines.append(stripped)
    
    if paragraph_lines:
        return ' '.join(paragraph_lines)[:200]
    
    return ""


def get_content_counts(source_path: str) -> dict:
    """Count MD files and other files in directory."""
    exclude_patterns = ['node_modules', '.git', '__pycache__', '.DS_Store', 'Thumbs.db']
    
    md_count = 0
    other_count = 0
    
    for root, dirs, files in os.walk(source_path):
        dirs[:] = [d for d in dirs if d not in exclude_patterns]
        
        for file in files:
            if file.endswith('.md'):
                md_count += 1
            else:
                other_count += 1
    
    return {
        'md_count': md_count,
        'other_count': other_count,
        'total': md_count + other_count
    }


def is_simple_skill(source_path: str) -> bool:
    """Check if source is a simple skill (has SKILL.md, no skills/ subdirectory).
    
    If there's no skills/ subdirectory, it's a simple skill regardless of other content
    (scripts/, references/, etc.).
    """
    skill_md_path = os.path.join(source_path, 'SKILL.md')
    skills_dir_path = os.path.join(source_path, 'skills')
    
    # Must have SKILL.md
    if not os.path.isfile(skill_md_path):
        return False
    
    # Must NOT have skills/ subdirectory
    # If skills/ exists, this is an extension containing sub-skills
    if os.path.isdir(skills_dir_path):
        return False
    
    # Has SKILL.md but no skills/ subdirectory → simple skill
    return True


def create_extension_manifest(name: str, description: str, output_path: str) -> str:
    """Create gemini-extension.json file.
    
    Format reference: example/UnityGameDevWorkflow/gemini-extension.json
    - No BOM (use utf-8 encoding without BOM)
    - Simple structure: name, version, description only
    """
    manifest = {
        "name": name,
        "version": "1.0.0",
        "description": description
    }
    
    json_path = os.path.join(output_path, 'gemini-extension.json')
    # Use utf-8 encoding WITHOUT BOM (Python's default utf-8 doesn't add BOM)
    with open(json_path, 'w', encoding='utf-8', newline='\n') as f:
        json.dump(manifest, f, indent=2, ensure_ascii=False)
    
    return json_path


def create_codely_md(name: str, description: str, body: str, output_path: str) -> str:
    """Create CODELY.md file."""
    content = f"""# {name} - Codely CLI Extension

{description}

{body}

---

*This extension was converted using skill-converter*
"""
    
    md_path = os.path.join(output_path, 'CODELY.md')
    with open(md_path, 'w', encoding='utf-8') as f:
        f.write(content)
    
    return md_path


def convert_skill_md_to_extension(skill_md_path: str, output_path: str, name: str):
    """Convert SKILL.md format to extension format."""
    with open(skill_md_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    description = ""
    body = content
    
    if content.startswith('---'):
        parts = content.split('---', 2)
        if len(parts) >= 3:
            frontmatter = parts[1]
            body = parts[2].strip()
            
            for line in frontmatter.split('\n'):
                if line.strip().startswith('description:'):
                    description = line.split(':', 1)[1].strip()
                    break
    
    if not description:
        description = f"Extension for {name}."
    
    create_extension_manifest(name, description, output_path)
    create_codely_md(name, description, body, output_path)


def install_as_skill(source_path: str, output_path: str, name: str) -> str:
    """Install as a skill."""
    print("Installing as SKILL...")
    
    if os.path.exists(output_path):
        shutil.rmtree(output_path)
    shutil.copytree(source_path, output_path)
    
    skill_md_path = os.path.join(output_path, 'SKILL.md')
    if not os.path.exists(skill_md_path):
        readme_path = os.path.join(output_path, 'README.md')
        description = f"Skill for {name}."
        
        if os.path.exists(readme_path):
            description = extract_description_from_readme(readme_path)
        
        skill_md_content = f"""---
name: {name}
description: {description}
---

# {name.replace('-', ' ').title()}

This skill was converted from a GitHub repository.
"""
        
        with open(skill_md_path, 'w', encoding='utf-8') as f:
            f.write(skill_md_content)
    
    return output_path


def install_as_extension(source_path: str, output_path: str, name: str) -> str:
    """Install as an extension."""
    print("Installing as EXTENSION...")
    
    os.makedirs(output_path, exist_ok=True)
    
    skill_md_path = os.path.join(source_path, 'SKILL.md')
    
    if os.path.exists(skill_md_path):
        convert_skill_md_to_extension(skill_md_path, output_path, name)
    else:
        readme_path = os.path.join(source_path, 'README.md')
        description = f"Extension for {name}."
        body = ""
        
        if os.path.exists(readme_path):
            description = extract_description_from_readme(readme_path)
            with open(readme_path, 'r', encoding='utf-8') as f:
                body = f.read()
        
        create_extension_manifest(name, description, output_path)
        create_codely_md(name, description, body, output_path)
    
    # Copy resource directories (including skills/ for extension with sub-skills)
    dirs_to_copy = ['scripts', 'references', 'templates', 'assets', 'src', 'examples', 'shared', 'skills']
    for dir_name in dirs_to_copy:
        src_dir = os.path.join(source_path, dir_name)
        if os.path.isdir(src_dir):
            dst_dir = os.path.join(output_path, dir_name)
            if os.path.exists(dst_dir):
                shutil.rmtree(dst_dir)
            shutil.copytree(src_dir, dst_dir)
            print(f"  Copied: {dir_name}/")
    
    # Copy README if exists
    readme_src = os.path.join(source_path, 'README.md')
    if os.path.exists(readme_src):
        shutil.copy2(readme_src, os.path.join(output_path, 'README.md'))
    
    return output_path


def main():
    parser = argparse.ArgumentParser(description='Convert a GitHub repository to a Codely CLI skill or extension')
    parser.add_argument('url', help='GitHub URL to convert')
    parser.add_argument('--scope', '-s', choices=['project', 'global'], default='project',
                        help='Install scope: project (default) or global')
    parser.add_argument('--output', '-o', help='Custom output directory (overrides scope)')
    parser.add_argument('--name', '-n', help='Skill/extension name')
    parser.add_argument('--as-extension', action='store_true', help='Force conversion as extension')
    parser.add_argument('--as-skill', action='store_true', help='Force conversion as skill')
    parser.add_argument('--overwrite', action='store_true', help='Overwrite existing directory')
    
    args = parser.parse_args()
    
    try:
        # Parse URL
        github_info = parse_github_url(args.url)
        print(f"Parsed: owner={github_info['owner']}, repo={github_info['repo']}, "
              f"branch={github_info['branch']}, path={github_info['path']}")
        
        # Create temp directory
        with tempfile.TemporaryDirectory() as temp_dir:
            # Download repo
            extracted_path = download_github_repo(
                github_info['owner'],
                github_info['repo'],
                github_info['branch'],
                temp_dir
            )
            print(f"Extracted to: {extracted_path}")
            
            # Navigate to subfolder if specified
            if github_info['path']:
                source_path = os.path.join(extracted_path, github_info['path'])
            else:
                source_path = extracted_path
            
            if not os.path.exists(source_path):
                raise ValueError(f"Path not found in repository: {github_info['path']}")
            
            # Determine name
            if args.name:
                name = to_kebab_case(args.name)
            else:
                name = to_kebab_case(os.path.basename(source_path))
            
            print(f"Name: {name}")
            
            # Determine type
            if args.as_skill:
                is_skill = True
                print("Type: SKILL (forced)")
            elif args.as_extension:
                is_skill = False
                print("Type: EXTENSION (forced)")
            else:
                is_skill = is_simple_skill(source_path)
                counts = get_content_counts(source_path)
                if is_skill:
                    print("Type: SKILL (single MD file detected)")
                else:
                    print(f"Type: EXTENSION (complex content: {counts['md_count']} MD files, {counts['other_count']} other files)")
            
            # Determine output path
            type_dir = 'skills' if is_skill else 'extensions'
            
            if args.output:
                output_path = os.path.join(args.output, name)
            elif args.scope == 'global':
                output_path = os.path.join(Path.home(), '.codely-cli', type_dir, name)
            else:
                project_root = os.getcwd()
                output_path = os.path.join(project_root, '.codely-cli', type_dir, name)
            
            print(f"Output path: {output_path}")
            
            # Check overwrite
            if os.path.exists(output_path):
                if not args.overwrite:
                    print(f"\nError: '{output_path}' already exists.")
                    print("Use --overwrite to replace it.")
                    return 1
                print("Overwriting existing directory...")
                shutil.rmtree(output_path)
            
            # Install
            if is_skill:
                install_as_skill(source_path, output_path, name)
            else:
                install_as_extension(source_path, output_path, name)
            
            print(f"\nSuccess: {name} installed to {output_path}")
            print("\nRun '/skills reload' or '/extensions reload' in Codely CLI to enable.")
            
    except Exception as e:
        print(f"Error: {e}")
        return 1
    
    return 0


if __name__ == '__main__':
    exit(main())