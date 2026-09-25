---
name: unity-tts-generation
description: Synthesize spoken dialogue or narration with the generator MCP and import the result as a Unity AudioClip. Use for text-to-speech, NPC dialogue, and speech with an existing voice ID. Voice cloning uses unity-voice-clone; music and sound effects use their audio skills.
---

> **执行前读取**：[共享执行约定](../../experience/templates/generator-async-pattern.md)。
> 主 agent 按需委托 `general-purpose`；已在子代理中则直接执行本 skill，不再委托。已提交的阶段从原任务续接。

# Speech in Unity

Generate through MCP `generate_tts`, then import through Unity `import_audio_from_url`. The removed Unity `generate_tts` custom tool is no longer available.

1. Call MCP `generate_tts` with the exact text in `prompt` and `audio_format="mp3"`. Preserve a user-selected `voice_id`, including an ID returned by `voice_clone`. Omit unspecified voice settings so the backend supplies its defaults. Use the live MCP schema for supported `language_boost`, `emotion`, `speed`, `pitch`, `vol`, and `sample_rate` values.
2. Follow the generation response's background-wait instructions for the returned task ID. After that wait finishes, use `check_task` to confirm success and obtain `output.data.audioUrl`. Do not submit the same speech again while it is pending.
3. Call `execute_custom_tool("import_audio_from_url", {audio_url, prompt, is_bgm:false, play_on_awake:false, output_path})`. Omit `output_path` unless the user supplied one. Record the import task ID and placeholder path.
4. Run a foreground shell wait of at most 30 seconds in this subagent, then call `query_local_task` once. If it is still importing or recovering, wait again for that same import. Do not end the subagent or call `complete_task` while pending. A timer completing is not evidence that the audio import completed.
5. After `status="completed"`, verify that the returned `audio_path` loads as an AudioClip. Bind it to the requested target only when placement was requested. Speech does not loop; spatial audio depends on whether it belongs to a character or UI/narration.

MCP TTS uses `audio_format`, not the music/SFX `output_format` parameter. Choose MP3 for Unity: raw PCM and FLAC are not accepted by this import tool. Report a format mismatch instead of automatically generating and charging for the same speech again.

Report both task IDs, `audio_path`, actual clip duration, and any AudioSource changes. Submission, a silent placeholder, and a background timer ending are pending states, not a successful final result. Follow MCP cost-confirmation responses when present.
