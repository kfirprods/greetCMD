using System;
using System.Linq;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared.Math;
using GTALife.Gamemode.Library.FunctionLibraries;

namespace GTALife.Gamemode.Features.Animations
{
    public class GreetingType
    {
        public string Title { get; set; }
        public string AnimationCategory { get; set; }
        public string AnimationName { get; set; }

        public GreetingType(string title, string animCategory, string anim)
        {
            this.Title = title;
            this.AnimationCategory = animCategory;
            this.AnimationName = anim;
        }
    }

    public class Greet : Script
    {
        private const float GreetingMaxDistance = 1.2f;
        private const int GreetingAnimationFlags = (int)(AnimHandler.AnimationFlags.StopOnLastFrame | AnimHandler.AnimationFlags.Cancellable);
        private const string GreetingSenderEntityDataKey = "GREET_SENDER";
        private const string GreetingTypeSelectionEntityDataKey = "GREET_ANIM";

        private readonly GreetingType[] _greetingTypes;

        public Greet()
        {
            this._greetingTypes = new []
            {
                new GreetingType("Handshake", "mp_ped_interaction", "handshake_guy_a"),
                new GreetingType("Kiss", "mp_ped_interaction", "kisses_guy_a"),
                new GreetingType("High Five", "mp_ped_interaction", "highfive_guy_a"),
                new GreetingType("Hug", "mp_ped_interaction", "hugs_guy_a"),
                new GreetingType("Gentle Nod", "mp_cp_welcome_tutgreet", "greet"),
                new GreetingType("Wave", "rcmepsilonism8", "security_greet"),
            };

            API.onPlayerDisconnected += OnPlayerQuit;
        }


        [Command("greet", Group = "Player Commands")]
        public void GreetCommand(Client sender, string targetNameOrId)
        {
            var target = PlayerLibrary.CommandClientFromString(API, sender, targetNameOrId);
            if (target == null) return;

            if (sender == target)
            {
                API.sendChatMessageToPlayer(sender, Resources.General.cant_greet_self);
                return;
            }

            if (GetDistance(sender.position, target.position) <= GreetingMaxDistance)
            {
                // If the player has greet data then we have to reset the old greeting information off from the target
                if (sender.hasData(GreetingSenderEntityDataKey)) 
                {
                    var oldTarget = sender.getData(GreetingSenderEntityDataKey);
                    if (oldTarget.exists)
                    {
                        oldTarget.resetData(GreetingSenderEntityDataKey);
                    }
                }

                ShowGreetSelectionMenu(sender, target);
            }
            else
            {
                API.sendChatMessageToPlayer(sender, Resources.Command.too_far);
            }
        }

        [Command("acceptgreet")]
        public void AcceptGreet(Client target)
        {
            if (target.hasData(GreetingSenderEntityDataKey))
            {
                var sender = target.getData(GreetingSenderEntityDataKey);
                if (GetDistance(sender.position, target.position) <= GreetingMaxDistance)
                {
                    DoGreeting(sender, target, target.getData(GreetingTypeSelectionEntityDataKey));
                    ResetPlayersGreetData(sender,
                        target); // At this point the greeting has been completed and so the data are reset
                }
                else
                {
                    API.sendChatMessageToPlayer(target, Resources.Command.too_far);
                }
            }
            else
            {
                API.sendChatMessageToPlayer(target, Resources.General.no_greeting_requests);
            }
        }

        public void DoGreeting(Client sender, Client target, GreetingType type)
        {
            // Rotate the sender towards the target
            API.setEntityRotation(sender,
                new Vector3(sender.rotation.X, sender.rotation.Y, Vector3ToAngle(sender.position, target.position)));

            // Rotate the target towards the sender
            API.setEntityRotation(target, new Vector3(target.rotation.X, target.rotation.Y, sender.rotation.Z + 180f));

            // Play the same animation for both players
            sender.playAnimation(type.AnimationCategory, type.AnimationName, GreetingAnimationFlags);
            target.playAnimation(type.AnimationCategory, type.AnimationName, GreetingAnimationFlags);
        }

        private void SetPlayersGreetData(Client sender, Client target)
        {
            sender.setData(GreetingSenderEntityDataKey, target);
            target.setData(GreetingSenderEntityDataKey, sender);
        }

        private void ResetPlayersGreetData(Client sender, Client target)
        {
            target.resetData(GreetingSenderEntityDataKey);
            sender.resetData(GreetingSenderEntityDataKey);
        }

        private void ShowGreetSelectionMenu(Client player, Client target)
        {
            var greetingMenu = new MenuContent("Greetings", "Greetings", "Choose greeting");

            greetingMenu.AddRange(this._greetingTypes.Select(greetingType => 
                new SelectMenuItem(greetingType.Title, (api, client, args) =>
                {
                    SetPlayersGreetData(client, target);
                    client.sendChatMessage(string.Format(Resources.General.greeting_request_outgoing, greetingType.Title, NamingFunctions.RoleplayName(target)));
                    target.sendChatMessage(string.Format(Resources.General.greeting_request_incoming, NamingFunctions.RoleplayName(client), greetingType.Title));

                    target.setData(GreetingTypeSelectionEntityDataKey, greetingType);
                    return true;
                })));

            MenuLibrary.OpenMenuContent(this.API, player, greetingMenu, false);
        }

        private void OnPlayerQuit(Client player, string reason)
        {
            if (player.hasData(GreetingSenderEntityDataKey))
            {
                ResetPlayersGreetData(player, player.getData(GreetingSenderEntityDataKey));
            }
        }

        public static float GetDistance(Vector3 pos1, Vector3 pos2)
        {
            return (float) Math.Sqrt(Math.Pow(pos2.X - pos1.X, 2) + Math.Pow(pos2.Y - pos1.Y, 2) +
                                     Math.Pow(pos2.Z - pos1.Z, 2));
        }

        public static float Vector3ToAngle(Vector3 position, Vector3 heading)
        {
            float angle = (float) Math.Atan2(heading.Y - position.Y, heading.X - position.Y);
            angle = angle * (180f / (float) Math.PI);

            return angle;
        }
    }
}
