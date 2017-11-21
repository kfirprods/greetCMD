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
        if (GetDistance(sender.position, target.position) <= greetingMaxDistance)
        {
            if (sender.hasData("GREET_PLAYER"))
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
                ResetPlayersGreetData(sender, target);
                DoGreeting(sender, target, target.getData("GREET_ANIM"));
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
        sender.rotation.Z = DirectionToHeading(target.position);
        target.rotation.Z = DirectionToHeading(sender.position);

        // Playing the same animation for both players
        int flags = (int)(AnimationFlags.StopOnLastFrame | AnimationFlags.AllowPlayerControl | AnimationFlags.Cancellable);

        switch (type){
            case 0:
                API.playPlayerAnimation(sender, flags, "anim@mp_player_intcelebrationpaired@f_m_manly_handshake", "manly_handshake_right_facial");
                API.playPlayerAnimation(target, flags, "anim@mp_player_intcelebrationpaired@f_m_manly_handshake", "manly_handshake_right_facial");
                break;
            case 1:
                API.playPlayerAnimation(sender, flags, "anim@mp_player_intcelebrationpaired@f_f_fist_bump", "fist_bump_right_facial");
                API.playPlayerAnimation(target, flags, "anim@mp_player_intcelebrationpaired@f_f_fist_bump", "fist_bump_right_facial");
                break;
            case 2:
                API.playPlayerAnimation(sender, flags, "anim@mp_player_intcelebrationpaired@f_m_high_five", "high_five_right_facial");
                API.playPlayerAnimation(target, flags, "anim@mp_player_intcelebrationpaired@f_m_high_five", "high_five_right_facial");
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
                player.setData("GREET_ANIM", arguments[0]);
                Client target = player.getData("GREET_PLAYER");

                switch (arguments[0])
                {
                    case 0:
                        API.sendChatMessageToPlayer(target, target.name + " has sent you a handshake request. ' /acceptgreet ' to accept.");
                        break;
                    case 1:
                        API.sendChatMessageToPlayer(target, target.name + " has sent you a fist bump request. ' /acceptgreet ' to accept.");
                        break;
                    case 2:
                        API.sendChatMessageToPlayer(target, target.name + " has sent you a high five request. ' /acceptgreet ' to accept.");
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

    public static float RadiansToDegrees(float radian)
    {
        return radian * (180.0f / (float)Math.PI);
    }

    public static float DirectionToHeading(Vector3 dir)
    {
        dir.Z = 0.0f;
        dir.Normalize();
        return RadiansToDegrees((float)Math.Atan2(dir.X, dir.Y));
    }

    public static float GetDistance(Vector3 pos1, Vector3 pos2)
    {
        return (float)Math.Sqrt(Math.Pow(pos2.X - pos1.X, 2) + Math.Pow(pos2.Y - pos1.Y, 2) + Math.Pow(pos2.Z - pos1.Z, 2));
    }
}