Dear Developer,

Thank you for your support.

As you know, Asset validation is an important part of any project, particularly as it grows in assets and team size. Ensuring all the prefabs, textures, materials, audio, etc., are properly and optimally configured can not only be a major time saver in debugging issues, but also often leads to major savings in runtime memory, load times, and build size.

This package contains a powerful validation framework which supports 3 types of validators:
1. Asset validators - For validating any asset type such as materials, textures, audio clips, or scriptable objects.
2. Component validators - For validating components on prefabs or objects within scenes.
3. Field validators - For validating attributes on component properties. There are 2 pre-written ones:
  [Required] attribute requires the property to be a non-null value.
  [ResourcePath] attribute requires the field to be a string or object in a Resources folder.

Included also is a demo project, with sample validators you can reference when customizing the validators to suit your own project needs. Please read "Demo Instructions.md".

You can run the Asset Validator from the menu:
  Menu->Window->Asset Validator
  or use hotkey Ctrl/Cmd-Shift + V

Alternatively, you can also run it as a unit test from:
  Menu->General->Test Runner->AssetValidatorTests
  This is a text interface intended for Continuous Integration build processes.

As a last tip, I also recommend using Unity's AssetPostprocess API in tandem with AssetValidator as it can also automatically configure and enforce settings at import time, although only for certain asset types. AssetValidator provides additional flexible and sophisticated checks across all asset types and components.

And that's it, I hope you love it!
If you have any questions, please drop us a line:
Discord: https://discord.gg/GQQepKuM5K
Unity Forum: https://discussions.unity.com/t/released-asset-validator/935102

We can't wait to see what you'll create next!

OmniShade
contact@omnishade.io
