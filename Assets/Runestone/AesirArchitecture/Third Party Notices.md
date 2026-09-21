This package contains third-party software components governed by the license(s) indicated below:

Component Name: QFramework

License Type: "MIT"

[QFramework License](https://github.com/liangxiegame/QFramework/blob/master/LICENSE)


Component Name: ObservableCollections (Cysharp)

License Type: "MIT"

Copyright (c) 2021 Cysharp, Inc.

[ObservableCollections License](https://github.com/Cysharp/ObservableCollections/blob/master/LICENSE)

Port reference — the `CollectionChanged` notification model (`IObservableCollection<T>`, `NotifyCollectionChangedEventArgs<T>`,
`SortOperation<T>`) in this package is ported from Cysharp/ObservableCollections, translated to this package's namespace and naming
conventions and adapted for Unity 2022.3 (C# 9). Upstream is distributed via NuGet; this package ships a ported source subset under
the MIT license, with attribution. This package intentionally implements only a high-frequency subset (four collections, no
synchronized views); the remaining upstream capabilities (synchronized views, R3 extensions, ring buffers, alternate index list,
`INotifyCollectionChanged` binding layer, writable views) are not included — use the upstream package for those.
Deviations from upstream are itemized in `Documentation/observable-collections.md` (Unsafe/CollectionsMarshal fast paths replaced
with allocation-free fallbacks, `record struct` / primary constructors downgraded to C# 9 syntax).



Component Name: Vertical 2D Shooting (Goldmetal)

License Type: "Free for commercial use with attribution"

Copyright ⓒ 2021 Goldmetal. Free to use (including commercially) provided that Goldmetal is credited as the source.

Sprite assets used by the PlaneWar sample (self-contained copies under Samples/PlaneWar) come from this pack.

[Goldmetal Studio](https://www.goldmetal.co.kr)
