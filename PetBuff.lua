function PetBuffSpell(spell)
	local InBattleFlag = spell.being.battleGrid.InBattle()
	local petlist = {}
	local buffnumber
	local morebuffs
	local petidsnumber
	local morepetids
	local duration
	local extra
	local amount
	local i
	local j
	local k
	if InBattleFlag == false then --This checks if the spell is set to trigger while not in battle. If In Battle, checking is moot anyway.
		if spell.HasParam("PetBuffInBattleOnly") then
			if spell.Param("PetBuffInBattleOnly") == "False" or spell.Param("PetBuffInBattleOnly") == "false" or spell.Param("PetBuffInBattleOnly") == "FALSE" then
				local InBattleFlag = true
			else
				if InBattleFlag == false then
					spell.being.MustBeInBattleWarning()
				end
			end
		end
	end
	if InBattleFlag then
		if spell.HasParam("PetBuffTarget") then
			if spell.Param("PetBuffTarget") == "OnlyMyPets" then
				if #(spell.being.currentPets) > 0 then
					for i = 0, #(spell.being.currentPets)-1, 1 do
						petlist[#petlist+1] = spell.being.currentPets[i]
					end
				end
			else
				if spell.Param("PetBuffTarget") == "AllEnemyPets" then
					if #(spell.being.battleGrid.currentEnemies) > 0 then
						for i = 0, #(spell.being.battleGrid.currentEnemies)-1, 1 do
							if #(spell.being.battleGrid.currentEnemies[i].currentPets) > 0 then
								for j = 0, #(spell.being.battleGrid.currentEnemies[i].currentPets)-1, 1 do
									petlist[#petlist+1] = spell.being.battleGrid.currentEnemies[i].currentPets[j]
								end
							end
						end
					end
				else
					if spell.Param("PetBuffTarget") == "AllPlayerPets" then
						for i = 1, #(spell.ctrl.currentPlayers), 1 do
							for j = 0, #(spell.ctrl.currentPlayers[i].currentPets)-1, 1 do
								petlist[#petlist+1] = spell.ctrl.currentPlayers[i].currentPets[j]
							end
						end
					else
						if spell.Param("PetBuffTarget") == "TargetsPets" then
							for _, target in ipairs(GetTarget(spell)) do
								if #(target.currentPets) > 0 then
									for i = 0, #(target.currentPets)-1, 1 do
										petlist[#petlist+1] = target.currentPets[i]
									end
								end
							end
						else
							if spell.Param("PetBuffTarget") == "NamedPetID" then
								petidsnumber = 0
								morepetids = true
								while morepetids do
									if spell.HasParam("PetID"..tostring(petidsnumber+1)) then
										petidsnumber = petidsnumber +1
									else
										morepetids = false
									end
								end
								for i = 1, #(spell.ctrl.currentPlayers), 1 do
									for j = 0, #(spell.ctrl.currentPlayers[i].currentPets)-1, 1 do
										for k = 1, petidsnumber ,1 do
											if spell.ctrl.currentPlayers[i].currentPets[j].beingObj.get_beingID() == spell.Param("PetID"..tostring(k)) then
												petlist[#petlist+1] = spell.ctrl.currentPlayers[i].currentPets[j]
											end
										end
									end
								end
								if #(spell.being.battleGrid.currentEnemies) > 0 then
									for i = 0, #(spell.being.battleGrid.currentEnemies)-1, 1 do
										if #(spell.being.battleGrid.currentEnemies[i].currentPets) > 0 then
											for j = 0, #(spell.being.battleGrid.currentEnemies[i].currentPets)-1, 1 do
												for k = 1, petidsnumber ,1 do
													if spell.being.battleGrid.currentEnemies[i].currentPets[j].beingObj.get_beingID() == spell.Param("PetID"..tostring(k)) then
														petlist[#petlist+1] = spell.being.battleGrid.currentEnemies[i].currentPets[j]
													end
												end
											end
										end
									end
								end
							end
						end
					end
				end
			end
		end
		if #petlist > 0 then
			buffnumber = 0
			morebuffs = true
			while morebuffs do
				if spell.HasParam("PetBuffStatus"..tostring(buffnumber+1)) then
					buffnumber = buffnumber +1
				else
					morebuffs = false
				end
			end
			if buffnumber > 0 then
				for i = 1, #petlist, 1 do
					for j = 1, buffnumber, 1 do
						if spell.HasParam("PetBuffAmount"..tostring(j)) then
							amount = tonumber(spell.Param("PetBuffAmount"..tostring(j)))
						else
							amount = 0
						end
						if  spell.HasParam("PetBuffDuration"..tostring(j)) then
							duration = tonumber(spell.Param("PetBuffDuration"..tostring(j)))
						else
							duration = 9999
						end
						if  spell.HasParam("PetBuffExtra"..tostring(j)) then
							extra = spell.Param("PetBuffExtra"..tostring(j))
						else
							extra = "None"
						end
						BuffTarget(petlist[i], spell.Param("PetBuffStatus"..tostring(j)),amount,duration,extra)
				
					end
				end
			end
		end
	end	
end
function BuffTarget(being, status, amount, duration, extra)
	local spell
	if status == "Heal" then
		being.Heal(math.floor(amount))
	else
		if status == "DamageNorm" then
			being.Damage(amount)
		else
			if status == "Damage" then
				being.Damage(amount, true)
			else
				if status == "DamageTrue" then
					being.Damage(amount, true, true)
				else
					if status == "Invincibility" then
						being.AddInvince(duration)
					else
						if status == "CastSpell" then
							if extra ~= "None" then
								spell = being.deCtrl.CreateSpellBase(extra, being, false)
								spell.interrupt = false
								spell.StartCast(false, 0, false)
							end
						else
							if extra == "Cleanse" then
								being.RemoveStatus(status)
							else
								being.AddStatus(status, amount, duration)
							end
						end
					end
				end
			end
		end
	end
end
function PetBuffArtifactFunction(item, target, status, amount, duration, extra)
	local petlist = {}
	local i
	local j
	local localamount
	local localduration
	local localextra
	if target == "OnlyMyPets" then
		if #(item.being.currentPets) > 0 then
			for i = 0, #(item.being.currentPets)-1, 1 do
				petlist[#petlist+1] = item.being.currentPets[i]
			end
		end
	else
		if target == "AllEnemyPets" then
			if #(item.being.battleGrid.currentEnemies) > 0 then
				for i = 0, #(item.being.battleGrid.currentEnemies)-1, 1 do
					if #(item.being.battleGrid.currentEnemies[i].currentPets) > 0 then
						for j = 0, #(item.being.battleGrid.currentEnemies[i].currentPets)-1, 1 do
							petlist[#petlist+1] = item.being.battleGrid.currentEnemies[i].currentPets[j]
						end
					end
				end
			end
		else
			if target == "AllPlayerPets" then
				for i = 1, #(item.ctrl.currentPlayers), 1 do
					for j = 0, #(item.ctrl.currentPlayers[i].currentPets)-1, 1 do
						petlist[#petlist+1] = item.ctrl.currentPlayers[i].currentPets[j]
					end
				end
			else
				if target== "TargetsPets" then
					for _, target in ipairs(GetTarget(item)) do
						if #(target.currentPets) > 0 then
							for i = 0, #(target.currentPets)-1, 1 do
								petlist[#petlist+1] = target.currentPets[i]
							end
						end
					end
				end
			end
		end
	end
	if #petlist > 0 then
		for i = 1, #petlist, 1 do
			if amount ~= nil then
				localamount = amount
			else
				amount = 0
			end
			if duration ~= nil then
				localduration = duration
			else
				localduration = 9999
			end
			if extra ~= nil then
				localextra = extra
			else
				localextra = "None"
			end
			BuffTarget(petlist[i], status, localamount, localduration, localextra)
		end
	end
end

function PetBuffArtifactPetIDFunction(item, petid, status, amount, duration, extra)
	local petlist = {}
	local i
	local j
	local localamount
	local localduration
	local localextra
	for i = 1, #(item.ctrl.currentPlayers), 1 do
		for j = 0, #(item.ctrl.currentPlayers[i].currentPets)-1, 1 do
			if item.ctrl.currentPlayers[i].currentPets[j].beingObj.get_beingID() == petid then
				petlist[#petlist+1] = item.ctrl.currentPlayers[i].currentPets[j]
			end
		end
	end
	if #(item.being.battleGrid.currentEnemies) > 0 then
		for i = 0, #(item.being.battleGrid.currentEnemies)-1, 1 do
			if #(item.being.battleGrid.currentEnemies[i].currentPets) > 0 then
				for j = 0, #(item.being.battleGrid.currentEnemies[i].currentPets)-1, 1 do
					if item.being.battleGrid.currentEnemies[i].currentPets[j].beingObj.get_beingID() == petid then
						petlist[#petlist+1] = item.being.battleGrid.currentEnemies[i].currentPets[j]
					end
				end
			end
		end
	end
	if #petlist > 0 then
		for i = 1, #petlist, 1 do
			if amount ~= nil then
				localamount = amount
			else
				amount = 0
			end
			if duration ~= nil then
				localduration = duration
			else
				localduration = 9999
			end
			if extra ~= nil then
				localextra = extra
			else
				localextra = "None"
			end
			BuffTarget(petlist[i], status, localamount, localduration, localextra)
		end
	end
end


function MyPetBuffArtifact(item)
	PetBuffArtifactFunction(item, "OnlyMyPets", "Defense", 10, 10, "None")
end