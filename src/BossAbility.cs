using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AbilityChanger;
namespace BossAbilities
{
    public abstract class BossAbility: Ability
    {
        public BossAbility() { }
        public abstract string abilityReplaced { get; }
        public abstract bool gotAbility { get; set; }
        public override bool hasAbility() => gotAbility;
        public virtual List<(string, string)> prefabs { get; }
        public virtual void Initialize() { }
        public virtual void GiveAbility()=> gotAbility = true;
        public virtual void TakeAbility() => gotAbility = false;


    }
}
