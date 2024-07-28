<p>
  <a href="https://steamcommunity.com/sharedfiles/filedetails/?id=YOUR_WORKSHOP_ITEM_ID">
  <img alt="Steam Workshop Link" src="https://img.shields.io/static/v1?label=Steam&message=Workshop&color=blue&logo=steam&link=https://steamcommunity.com/sharedfiles/filedetails/?id=YOUR_WORKSHOP_ITEM_ID"/>
  </a>
</p>

# [WIP] Inbetween - A Rimworld Roguelite experience

The In-Between is a space in-between realities. It doesn't follow the normal rules of reality. Be careful if you find youself there, you'll never know what to expect next!

This mod provides a framework for the "In-between". Utilizing `InbetweenZoneDef` any mod can register a potential zone to randomly spawn.

Currently the `InbetweenZoneDef` is very basic, - it only takes a `defName`, and a `MapGeneratorDef`. At some point, I want to extend that with values and variables to allow
tweaking the chance of the map to spawn, the chance based on difficulty to try to avoid super difficult ones up front, potential scaling, etc.

[InbetweenGameComponent](https://github.com/keyz182/Rimworld-Inbetween/blob/main/1.5/Source/Inbetween/Mapping/InbetweenGameComponent.cs) acts as the manager for the In-between.
Currently, only one instance is allowed at a time.

Travel between zones is facilitated through the [Building_InbetweenDoor](https://github.com/keyz182/Rimworld-Inbetween/blob/main/1.5/Source/Inbetween/Mapping/Building_Door.cs)
and [Building_ReturnDoor](https://github.com/keyz182/Rimworld-Inbetween/blob/main/1.5/Source/Inbetween/Mapping/Building_ReturnDoor.cs).

By default, only 3 pocket maps are kept. Travel to the next map will be blocked until all your pawns are within 2 maps of the newest. Once traveled, the older maps will be
destroyed.

## Disclaimer
Portions of the materials used to create this content/mod are trademarks and/or copyrighted works of Ludeon Studios Inc. All rights reserved by Ludeon. This content/mod is not official and is not endorsed by Ludeon.

## Thanks
* Ludeon for the Game
* Marnador for the [Rimworld Font](https://github.com/spdskatr/RWModdingResources/raw/master/RimWordFont.ttf)
