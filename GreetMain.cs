using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;

using GrandTheftMultiplayer.Shared.Math;
using System;

[Flags]
public enum AnimationFlags
{
    Loop = 1 << 0,
    StopOnLastFrame = 1 << 1,
    OnlyAnimateUpperBody = 1 << 4,
    AllowPlayerControl = 1 << 5,
    Cancellable = 1 << 7
}

public class GreetMain : Script
{
    public float greetingMaxDistance = 1.2f;

    public GreetMain()
    {
        API.onClientEventTrigger += OnClientEvent;
        API.onPlayerDisconnected += OnPlayerQuit;
    }


    [Command("greet")]
    public void Greet(Client sender, Client target)
    {
        if (sender == target)
        {
            API.sendChatMessageToPlayer(sender, "You can't greet yourself!");
            return;
        }

        if (GetDistance(sender.position, target.position) <= greetingMaxDistance) // Checking the distance between the duo
        {
            if (sender.hasData("GREET_PLAYER")) // If the player has greet data then we have to reset the old greeting
            {
                Client oldTarget = sender.getData("GREET_PLAYER");
                if (oldTarget.exists)
                {
                    oldTarget.resetData("GREET_PLAYER");
                    API.sendChatMessageToPlayer(oldTarget, "Your greeting request has been declined.");
                }
            }

            SetPlayersGreetData(sender, target);
            API.triggerClientEvent(sender, "GREET_MENU");
        }
        else
        {
            API.sendChatMessageToPlayer(sender, "You are too far away.");
        }
    }

    [Command("acceptgreet")]
    public void AcceptGreet(Client target)
    {
       if (target.hasData("GREET_PLAYER"))
        {
            Client sender = target.getData("GREET_PLAYER");
            if (GetDistance(sender.position, target.position) <= greetingMaxDistance)
            {
                DoGreeting(sender, target, target.getData("GREET_ANIM"));
                ResetPlayersGreetData(sender, target); // At this point the greeting has been completed and so the data are reset
            }
            else
            {
                API.sendChatMessageToPlayer(target, "You are too far away.");
            }
        }
        else
        {
            API.sendChatMessageToPlayer(target, "You have no pending greeting requests.");
        }
    }

    public void DoGreeting(Client sender, Client target, int type)
    {
        foreach (Client player in API.getPlayersInRadiusOfPlayer(10f, sender))
        {
            API.sendChatMessageToPlayer(player, sender.name + " is greeting " + target.name + ".");
        }

        // Rotating players to face each other
        API.setEntityRotation(sender, new Vector3(sender.rotation.X, sender.rotation.Y, Vector3ToAngle(sender.position, target.position)));
        API.setEntityRotation(target, new Vector3(target.rotation.X, target.rotation.Y, Vector3ToAngle(target.position, sender.position)));

        // Playing the same animation for both players
        int flags = (int)(AnimationFlags.StopOnLastFrame | AnimationFlags.Cancellable);

        switch (type){
            case 0:
                API.playPlayerAnimation(sender, flags, "mp_ped_interaction", "handshake_guy_a");
                API.playPlayerAnimation(target, flags, "mp_ped_interaction", "handshake_guy_a");
                break;
            case 1:
                API.playPlayerAnimation(sender, flags, "mp_ped_interaction", "kisses_guy_a");
                API.playPlayerAnimation(target, flags, "mp_ped_interaction", "kisses_guy_a");
                break;
            case 2:
                API.playPlayerAnimation(sender, flags, "mp_ped_interaction", "highfive_guy_a");
                API.playPlayerAnimation(target, flags, "mp_ped_interaction", "highfive_guy_a");
                break;
        }
    }

    private void SetPlayersGreetData(Client sender, Client target)
    {
        sender.setData("GREET_PLAYER", target);
        target.setData("GREET_PLAYER", sender);
    }

    private void ResetPlayersGreetData(Client sender, Client target)
    {
        target.resetData("GREET_PLAYER");
        sender.resetData("GREET_PLAYER");
    }

    private void OnClientEvent(Client player, string eventName, params object[] arguments)
    {
        switch (eventName) {
            case "GREET":
                Client target = player.getData("GREET_PLAYER");
                target.setData("GREET_ANIM", (int)arguments[0]);

                switch ((int)arguments[0])
                {
                    case 0:
                        API.sendChatMessageToPlayer(target, player.name + " has sent you a handshake request. ' /acceptgreet ' to accept.");
                        break;
                    case 1:
                        API.sendChatMessageToPlayer(target, player.name + " has sent you a kiss request. ' /acceptgreet ' to accept.");
                        break;
                    case 2:
                        API.sendChatMessageToPlayer(target, player.name + " has sent you a high five request. ' /acceptgreet ' to accept.");
                        break;
                }
                break;

            case "CANCEL_GREET":
                ResetPlayersGreetData(player, player.getData("GREET_PLAYER"));
                break;
        }
    }

    private void OnPlayerQuit(Client player, string reason)
    {
        if (player.hasData("GREET_PLAYER"))
        {
            ResetPlayersGreetData(player, player.getData("GREET_PLAYER"));
        }
    }

    public static float GetDistance(Vector3 pos1, Vector3 pos2)
    {
        return (float)Math.Sqrt(Math.Pow(pos2.X - pos1.X, 2) + Math.Pow(pos2.Y - pos1.Y, 2) + Math.Pow(pos2.Z - pos1.Z, 2));
    }

    public static float Vector3ToAngle(Vector3 position, Vector3 heading)
    {
        float angle = (float)Math.Atan2(heading.Y - position.Y, heading.X - position.Y);
        angle = angle * (180f / (float)Math.PI);

        return angle;
    }
}