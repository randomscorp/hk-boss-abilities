using System;
using Modding;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static AbilityChanger.AbilityChanger;

namespace BossAbilities {
    public class BossAbilities : Mod, IMenuMod {
        public BossAbilities() : base("BossAbilities") {}
        public override string GetVersion() => "v0";

        public static List<BossAbility> Abilities= new();
        public static Dictionary<string, Dictionary<string, GameObject>> Preloads;

        public static BossAbilities instance;

        public bool ToggleButtonInsideMenu => true;


        public override List<(string, string)> GetPreloadNames()
        {
            List<(string, string)> prefabs = new();
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                if (type.BaseType == typeof(BossAbility))
                {
                    Abilities.Add(Activator.CreateInstance(type) as BossAbility);
                    foreach(var name in Abilities[Abilities.Count-1].prefabs)
                    {
                        if(name.Item1 is not null && name.Item2 is not null && !prefabs.Contains(name))
                        prefabs.Add(name);
                    }
                }
            }
            return prefabs;
        }


        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) {
            
            
            instance= this;
            Preloads = preloadedObjects;
            
            foreach (var name in Preloads.Values)
            {
                Log(name);
            }
            LoadAbilities();


        }

        private void LoadAbilities() {
            foreach (BossAbility ability in Abilities) {

                try
                {
                    RegisterAbility(ability.abilityReplaced, ability);
                    Log($"Registered ability {ability.name}!");
                } 

                catch { Log($"Error on registering {ability.name}!"); }

                try
                {
                    ability.Initialize();
                    Log($"Initialized Ability {ability.name}");
                }
                catch { Log($"Error on Initializing {ability.name}"); }
            }
        }
        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            List<IMenuMod.MenuEntry> entries = new();
            foreach(BossAbility ability in Abilities)
            {
                entries.Add(
                    new IMenuMod.MenuEntry
                    {
                        Name = ability.name,
                        Description = ability.description,
                        Values = new string[] {
                            "Off",
                            "On"
                        },
                        Saver = opt => ability.gotAbility = opt switch
                        {
                            0 => false,
                            1 => true,
                            // This should never be called
                            _ => throw new InvalidOperationException()
                        },
                        Loader = () => ability.gotAbility switch
                        {
                            false => 0,
                            true => 1,

                        }
                    }
                    );
            }

        return entries;

        }


    }

}