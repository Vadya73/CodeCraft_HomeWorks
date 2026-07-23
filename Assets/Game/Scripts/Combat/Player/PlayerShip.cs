using Game.Scripts.Combat.Common;
using Game.Scripts.Combat.Ships;

namespace Game.Scripts.Combat.Player
{
    // +
    public sealed class PlayerShip : ShipController
    {
        public override TeamType Team => TeamType.Player;
    }
}
