This package contains third-party software components governed by the license(s) indicated below:

Component Name: QFramework

License Type: "MIT"

[QFramework License](https://github.com/liangxiegame/QFramework/blob/master/LICENSE)


Component Name: ObservableCollections (Cysharp)

License Type: "MIT"

Copyright (c) 2021 Cysharp, Inc.

[ObservableCollections License](https://github.com/Cysharp/ObservableCollections/blob/master/LICENSE)

Port reference — the observable-collection family in this package (`IObservableCollection<T>`, `NotifyCollectionChangedEventArgs<T>`,
`SynchronizedView` / `ISynchronizedView<T, TView>` / filter, `RingBuffer` / `AlternateIndexList`, the `INotifyCollectionChanged`
binding layer, and the R3 extension set) is a full port of Cysharp/ObservableCollections, translated to this package's namespace
and naming conventions and adapted for Unity 2022.3 (C# 9). Upstream is distributed via NuGet; this package ships a ported
source copy under the MIT license, with attribution. Deviations from upstream are documented in
`Documentation/observable-collections.md` (Unsafe/CollectionsMarshal fast paths replaced with allocation-free fallbacks,
`record struct` / primary constructors downgraded to C# 9 syntax, abstract class prefixed with `Abstract`).


Component Name: R3 (Cysharp)

License Type: "MIT"

Copyright (c) Cysharp, Inc.

[R3 License](https://github.com/Cysharp/R3/blob/master/LICENSE)

Optional integration dependency — the `Runestone.AesirArchitecture.R3` assembly ports the upstream `ObservableCollections.R3`
extension set and requires the `R3` assembly to be present in the project (install via NuGetForUnity or a plugin DLL). R3 itself
is NOT distributed with this package; the integration assembly is excluded from compilation (`AESIR_R3` define constraint) when
R3 is absent. The `R3` NuGet package carries its own dependency licenses (Microsoft.Bcl.AsyncInterfaces, Microsoft.Bcl.TimeProvider,
System.Threading.Channels, System.Runtime.CompilerServices.Unsafe — all MIT).


Component Name: Vertical 2D Shooting (Goldmetal)

License Type: "Free for commercial use with attribution"

Copyright ⓒ 2021 Goldmetal. Free to use (including commercially) provided that Goldmetal is credited as the source.

Sprite assets used by the PlaneWar sample (self-contained copies under Samples/PlaneWar) come from this pack.

[Goldmetal Studio](https://www.goldmetal.co.kr)
