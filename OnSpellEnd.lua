function Outro(spell, storedBeing)
    -- Standard Outro --
    if (storedBeing == nil) then
        storedBeing = spell.being
    end

    if (spell.anchor == true) then
        storedBeing.mov.SetState(State.Idle)
    end
    
    if (spell.fireLoop == FireLoop.While) then
        spell.beingAnim.SetBool("fireWhile", false)
    end

    -- Call ending function --
    if (spell.HasParam("onSpellEnd")) then
        _G[spell.Param("onSpellEnd")](spell)
    end
end