/*
 *  More Lua Power, made by Golden Epsilon
 *  Audio loading, ProgramAdvance, Kickers, and Multicast added by Sunreal
 *  PetBuff for MoreLuaPower by stephanreiken
 *  Subfolder loading by Shenanigans
 *  Workshop URL: https://steamcommunity.com/sharedfiles/filedetails/?id=2066319533
 *  GitHub Page: https://github.com/GoldenEpsilon/MoreLuaPower
 *
 *  Please do not include the DLL in your mods directly:
 *      Ask people to download the workshop version instead.
 *      
 *  That said, if there's something you want to modify from the code to make your own harmony mod, feel free!
 *  I am also open to help; If you have something you want to add in here, just let me know/add it in yourself! You will be credited.
*/

Hello! Thank you for looking at the readme.

I'm trying to make the code nice to read, but I tend to focus on function rather than form, so apologies if it's still messy.
If you have any recommendations for anything, from the API to the code to this readme, let me know! I'm [Golden Epsilon#8656] on discord.

To use the code:
 - Use Visual Studio (I'm using 2019)
 - Open the .sln file
 - Right click on MoreLuaPower (the csproj)
 - Go to Reference Paths
 - Add your version of steamapps\common\One Step From Eden\OSFE_Data\StreamingAssets\YourMods\MoreLuaPower
Then you should be good to go!

Files to look at:

    API.txt             is an explanation of all of the features of MoreLuaPower from the perspective of a lua modder. If you're here for making your mods crazier, this is the place to go.
    
    MoreLuaPower.dll    is the actual mod. If you need to test something that the workshop mod doesn't work for, this is the only file you need to move into the mods folder.
    
    README.txt          is the file you're reading right now! I'm going to use it as an overview so that I can send people here when I'm asked stuff.
    
    TODO.txt            is a file for my personal use - you're free to read it, though, if you want to take a look at what I'm thinking of adding next.
    
    MoreLuaPower.cs     is the main file for code organization. New functions will appear here first, as well, before being added to their own files.
    
    All other .cs files are the rest of the code, with as much documentation as I can. Each function should have a reasonable description, saying what it does and how it does it.

Thanks again for reading, have a nice day!
