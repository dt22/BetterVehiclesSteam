using PhoenixPoint.Modding;

namespace BetterVehicles
{
	/// <summary>
	/// ModConfig is mod settings that players can change from within the game.
	/// Config is only editable from players in main menu.
	/// Only one config can exist per mod assembly.
	/// Config is serialized on disk as json.
	/// </summary>
	public class BetterVehiclesConfig : ModConfig
	{
		/// Only public fields are serialized.
		/// Supported types for in-game UI are:
		[ConfigField(text: "Fix Text", description: "Fixes Display Text For Some Modules")]
		public bool FixText = true;

        [ConfigField(text: "Turn On Mutog Changes", description: "Various Changes To Mutog")]
        public bool TurnOnMutogChanges = true;
    }
}
