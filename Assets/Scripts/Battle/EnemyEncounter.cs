using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemyEncounter : MonoBehaviour {
    public LuaEnemyController[] enemies;
    public Vector2[] enemyPositions;
    internal float waveTimer;
    public int turnCount = 0;
    protected GameObject[] enemyInstances;

    public string EncounterText { get; set; }
    public bool CanRun { get; set; }

    public LuaEnemyController[] EnabledEnemies {
        get { return enemies.Where(x => x.inFight).ToArray<LuaEnemyController>(); }
    }

    public virtual Vector2 ArenaSize {
        get { return new Vector2(155, 130); }
    }

    protected virtual void LoadEnemiesAndPositions() { }

    protected string RandomEncounterText() {
        if (EnabledEnemies.Length <= 0)
            return "";
        int randomEnemy = UnityEngine.Random.Range(0, EnabledEnemies.Length);
        string[] comments;
        try { comments = EnabledEnemies[randomEnemy].Comments; }
        catch { throw new CYFException("RandomEncounterText: Can not read the \"comments\" table of enemy #" + (randomEnemy + 1).ToString() + ".\nAre you sure it's set?"); }
        if (comments.Length <= 0)
            return "";
        int randomComment = UnityEngine.Random.Range(0, comments.Length);
        return comments[randomComment];
    }

    public virtual void HandleItem(UnderItem item) {
        //if (!CustomItemHandler(item))
            //item.inCombatUse();
    }

    public virtual void HandleSpare() {
        //bool sparedAny = false;
        foreach (EnemyController enemy in enemies)
            if (enemy.CanSpare)
                enemy.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
                //sparedAny = true;
    }

    // <summary>
    // Overrideable item handler on a per-encounter basis. Should return true if a custom action is executed for the given item.
    // </summary>
    // <param name="item">Item to be checked for custom action</param>
    // <returns>true if a custom action should be executed for given item, false if the default action should happen</returns>
    /*public virtual bool CustomItemHandler(UnderItem item) {
        UIController.instance.encounter.CallOnSelfOrChildren("HandleItem", new MoonSharp.Interpreter.DynValue[] { MoonSharp.Interpreter.DynValue.NewString(item.Name) });
        return false;*/
        // the following was test code that allowed you to activate dogs in order 2-3-1 to replace all bullets with dogs
        /*if (dogTest[0] && dogTest[1] && dogTest[2])
        {
            UIController.instance.ActionDialogResult(new RegularMessage[]{
                new RegularMessage("After unlocking the\r[color:ffff00]Secret of Dog[color:ffffff],\ryou don't feel like using dog " + item.ID[7] + "."),
                new RegularMessage("So you released it.\nFarewell, dog!")
            }, UIController.UIState.ENEMYDIALOGUE);
            Inventory.container.Remove(item);
            return true;
        }

        if (item.ID == "DOGTEST2")
        {
            UIController.instance.ActionDialogResult(new RegularMessage("Selected dog 2.\nMight be part of a pattern."), UIController.UIState.ENEMYDIALOGUE);
            dogTest[0] = true;
            return true;
        }

        if (item.ID == "DOGTEST3" && dogTest[0])
        {
            UIController.instance.ActionDialogResult(new RegularMessage("Selected dog 3.\nThis seems about right..."), UIController.UIState.ENEMYDIALOGUE);
            dogTest[1] = true;
            return true;
        }

        if (item.ID == "DOGTEST1" && dogTest[1])
        {
            AudioClip yay = Resources.Load<AudioClip>("Sounds/dogsecret");
            AudioSource.PlayClipAtPoint(yay, Camera.main.transform.position);
            UIController.instance.ActionDialogResult(new RegularMessage[]{
                new RegularMessage("You have unlocked the\r[color:ffff00]Secret of Dog[color:ffffff].\nYou are overcome with happiness."),
                new RegularMessage("And spiders, too.")
            }, UIController.UIState.ENEMYDIALOGUE);
            dogTest[2] = true;
            return true;
        }

        if (dogTest[0] || dogTest[1] || dogTest[2])
            UIController.instance.ActionDialogResult(new RegularMessage("Selected dog " + item.ID[7] + ".\nNo... that's not it."), UIController.UIState.ENEMYDIALOGUE);
        else
            UIController.instance.ActionDialogResult(new RegularMessage("Selected dog " + item.ID[7] + ".\nIt feels off."), UIController.UIState.ENEMYDIALOGUE);

        dogTest[0] = false;
        dogTest[1] = false;
        dogTest[2] = false;
        return true;*/
    //}

    public virtual void UpdateWave() { }

    public virtual void NextWave() {
        turnCount++;
        waveTimer = Time.time + 4.0f;
        EncounterText = RandomEncounterText();
    }

    public virtual void EndWave(bool death = false) { }

    public bool WaveInProgress {
        get { return Time.time < waveTimer; }
    }
}