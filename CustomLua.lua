--Credit to the Custom Lua Effects mod for inspiration and part of the implementation
-- https://steamcommunity.com/sharedfiles/filedetails/?id=2063268758
function Lua (item)
	if (item.currentApp.value ~= nil and item.currentApp.value ~= '') then
		_G[item.currentApp.value](item)
	else
		print("ERROR: A Custom lua effect did not trigger correctly");
	end
end