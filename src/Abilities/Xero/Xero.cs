using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.GraphicsBuffer;
using HutongGames.PlayMaker.Actions;
using GlobalEnums;
using HutongGames.PlayMaker;
using UnityEngine;
using static Satchel.FsmUtil;
using System.Timers;
using System.Diagnostics;
using Satchel.Futils;

namespace BossAbilities.src.Abilities.Xero
{
    public class Xero : BossAbility
    {
        public override string abilityReplaced => AbilityChanger.Abilities.DREAMGATE;

        public override bool gotAbility { get; set; }
        public override string name { get => "Xero"; set { } }
        public override string title { get => "Xero"; set { } }
        public override string description { get => "Xero"; set { } }
        public override Sprite activeSprite { get => getActiveSprite(); set { } }
        public override Sprite inactiveSprite { get => getActiveSprite(); set { } }
        static Sprite getActiveSprite() { return AssemblyUtils.GetSpriteFromResources("placeholder.png"); }
        public override List<(string, string)> prefabs => new()
        {
               ("GG_Ghost_Xero","Warrior/Ghost Warrior Xero/Sword 3"),
               ("GG_Ghost_Xero","Warrior/Ghost Warrior Xero/S3 Home"),
               ("GG_Ghost_Xero","Warrior/Ghost Warrior Xero/Sword 4"),
               ("GG_Ghost_Xero","Warrior/Ghost Warrior Xero/S4 Home"),
        };
        public GameObject home1 = null;
        public GameObject home2 = null;
        public GameObject sword1 = null;
        public GameObject sword2 = null;
        private GameObject target = null;
        public static int damage = 80;

        public override void Initialize()
        {
            GiveAbility();
            On.PlayMakerFSM.OnEnable += FSMPatcher;


        }

        private void FSMPatcher(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            if (self.gameObject.name == "Knight" && self.FsmName == "Dream Nail")
            {

                self.GetState("Can Set?").InsertCustomAction(() =>
                {
                    if (enabled) self.SendEvent("FINISHED");

                }, 0);

                self.Intercept(new TransitionInterceptor()
                {
                    fromState = "Set",
                    toStateDefault = "Spawn Gate",
                    toStateCustom = "Set Recover",
                    eventName = "FINISHED",
                    shouldIntercept = () => enabled,
                    onIntercept = (a, b) => Complete()

                });
            }

        }

        private void Attack(On.EnemyDreamnailReaction.orig_RecieveDreamImpact orig, EnemyDreamnailReaction self)
        {
            orig(self);
            target = self.gameObject;

            if (sword1 != null)
            {
                var fsm = sword1.LocateMyFSM("xero_nail");
                fsm.GetState("Antic Spin").GetAction<GetAngleToTarget2D>(1).target = target;
                fsm.GetState("Antic Point").GetAction<GetAngleToTarget2D>(0).target = target;
                fsm.SendEvent("ATTACK");
                sword1.GetComponent<DamageEnemies>().enabled = true;
                if (sword2 != null)
                {
                    sword2.LocateMyFSM("xero_nail").GetState("Antic Spin").GetAction<GetAngleToTarget2D>(1).target = target;
                    sword2.LocateMyFSM("xero_nail").GetState("Antic Point").GetAction<GetAngleToTarget2D>(0).target = target;
                    sword2.LocateMyFSM("xero_nail").SendEvent("ATTACK");
                    sword2.GetComponent<DamageEnemies>().enabled = true;
                }
            }
        }

        private void AddHome()
        {
            home1 = UnityEngine.Object.Instantiate(BossAbilities.Preloads["GG_Ghost_Xero"]["Warrior/Ghost Warrior Xero/S3 Home"], HeroController.instance.transform);
            home1.name = home1.name.Replace("(Clone)", "");
            home1.SetActive(true);

            home2 = UnityEngine.Object.Instantiate(BossAbilities.Preloads["GG_Ghost_Xero"]["Warrior/Ghost Warrior Xero/S4 Home"], HeroController.instance.transform);
            home2.name = home2.name.Replace("(Clone)", "");
            home2.SetActive(true);
        }
        public void Complete()
        {
            if (sword1 == null)
            {

                sword1 = UnityEngine.Object.Instantiate(BossAbilities.Preloads["GG_Ghost_Xero"]["Warrior/Ghost Warrior Xero/Sword 3"], HeroController.instance.transform);
                SwordConfig(sword1, home1);
                return;
            }


            if (sword2 == null)
            {
                sword2 = UnityEngine.Object.Instantiate(BossAbilities.Preloads["GG_Ghost_Xero"]["Warrior/Ghost Warrior Xero/Sword 4"], HeroController.instance.transform);
                SwordConfig(sword2, home2);
                return;
            }
        }
        private void SwordConfig(GameObject sword, GameObject home)
        {
            var fsm = sword.LocateMyFSM("xero_nail");


            sword.SetActive(true);

            //sword.transform.localScale = new Vector3(1, -1, 1);

            sword.GetAddComponent<Collider2D>().isTrigger = true;
            sword.layer = (int)PhysLayers.HERO_ATTACK;
            var d = sword.GetAddComponent<DamageEnemies>();
            d.attackType = AttackTypes.Spell;
            d.damageDealt = damage;
            d.enabled = false;
            d.ignoreInvuln = false;

            sword.RemoveComponent<DamageHero>();

            fsm.GetState("Shoot").RemoveAction(5);

            fsm.GetState("Shoot").InsertCustomAction(() =>
            {
                fsm.GetState("Antic Spin").GetAction<GetAngleToTarget2D>(1).target = null;
                fsm.GetState("Antic Point").GetAction<GetAngleToTarget2D>(0).target = null;
            }, 0);

            fsm.GetState("Return Pause").GetAction<Wait>(0).time.Value = 0.1f;
            fsm.GetState("Return Pause").InsertCustomAction(() => sword.GetComponent<DamageEnemies>().enabled = false, 0);
            fsm.GetState("Home").InsertCustomAction(() => sword.GetComponent<DamageEnemies>().enabled = false, 0);
            //fsm.GetState("Returning").GetAction<FloatCompare>(5).float2.Value = home.transform.position.y ;
        }


        public override void OnSelect()
        {
            AddHome();
            On.EnemyDreamnailReaction.RecieveDreamImpact += Attack;
            ModHooks.GetPlayerBoolHook += EnableDreamNail;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += DestroySwords;

            enabled = true;

        }

        private void DestroySwords(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            try
            {
                UnityEngine.Object.Destroy(sword1);
                UnityEngine.Object.Destroy(sword2);
            }
            catch { }
        }

        public override void OnUnselect()
        {
            On.EnemyDreamnailReaction.RecieveDreamImpact -= Attack;
            ModHooks.GetPlayerBoolHook -= EnableDreamNail;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= DestroySwords;

            enabled = false;
        }

        private bool EnableDreamNail(string name, bool orig)
        {
            if (name != nameof(PlayerData.hasDreamNail)) return orig;
            return true;
        }
    }
}
