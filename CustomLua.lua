--Credit to the Custom Lua Effects mod for inspiration and part of the implementation
-- https://steamcommunity.com/sharedfiles/filedetails/?id=2063268758
function Lua (item)
	_G[item.currentApp.value](item)
end