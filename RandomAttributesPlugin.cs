using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using System;
using UnityEngine;

namespace RandomAttributes
{
    [BepInPlugin("com.dogoodogster.randomattributes", "Random Attributes", "1.1.0")]
    public class RandomAttributesPlugin : BaseUnityPlugin
    {
        private Harmony harmony;
        private static RandomAttributesPlugin instance;
        public static Attributes[] attributes = new Attributes[4];
        private void Awake()
        {
            instance = this;
            harmony = new Harmony(Info.Metadata.GUID);

            harmony.PatchAll(GetType());

            var bob = new Cow("Bob", "oogashaka", "culting");
            var joe = new Cow("Joe", "a communist", "voting for communist leaders");
            var Sam = new Cow("Sam", "the prankster", "playing tricks on other cows"); // totally not made by chatGPT in any way whatsoever.
            var Molly = new Cow("Molly", "the singer", "mooing melodiously"); // also totally not made by chatGPT in any way whatsoever.
            Logger.LogInfo(bob.Moo()); 
            Logger.LogInfo(joe.Moo());
            Logger.LogInfo(Sam.Moo());
            Logger.LogInfo(Molly.Moo());

            OnLevelStart += RandomizeThings;

            var color = new FixColor((Fix)110 / (Fix)255, (Fix)55 / (Fix)255, (Fix)74 / (Fix)255);
            color.ToHSV(out var h, out var s, out var v);
            var newColor = FixColor.FromHSV(h, s, v);
            Logger.LogInfo($"rgb({color.r * (Fix)255}, {color.g * (Fix)255}, {color.b * (Fix)255})");
            Logger.LogInfo($"hsv({h* (Fix)360}, {s * (Fix)100}, {v * (Fix)100})");
            Logger.LogInfo($"rgb({newColor.r * (Fix)255}, {newColor.g * (Fix)255}, {newColor.b * (Fix)255})");
        }

        public static event Action OnLevelStart;

        [HarmonyPatch(typeof(GameSessionHandler), nameof(GameSessionHandler.Init))]
        [HarmonyPostfix]
        private static void GameStart()
        {
            OnLevelStart();

            foreach (var player in PlayerHandler.Get().PlayerList())
            {
                player.Scale = attributes[player.Id - 1].scale;
            }
        }
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.UpdateSim))]
        [HarmonyPrefix]
        private static void PlayerPhysicsUpdate(PlayerPhysics __instance, ref IPlayerIdHolder ___playerIdHolder)
        {
            var slimecontroller = __instance.GetComponent<SlimeController>();

            if (slimecontroller != null)
            {
                slimecontroller.GetPlayerSprite().material.SetColor("_ShadowColor", attributes[___playerIdHolder.GetPlayerId() - 1].color);


                __instance.Speed = attributes[___playerIdHolder.GetPlayerId() - 1].speed;
                __instance.jumpStrength = attributes[___playerIdHolder.GetPlayerId() - 1].jumpStrength;
            }
        }
        public static void RandomizeThings()
        {
            for (int i = 0; i < attributes.Length; i++)
            {
                attributes[i] = new Attributes()
                {
                    scale = Updater.RandomFix((Fix)(-1), (Fix)3),
                    speed = Updater.RandomFix((Fix)1, (Fix)87.2),
                    jumpStrength = Updater.RandomFix((Fix)9.3, (Fix)109.8),
                    color = new FixColor(Updater.RandomFix((Fix)0, (Fix)1), Updater.RandomFix((Fix)0, (Fix)1), Updater.RandomFix((Fix)0, (Fix)1))
                };
            }
        }
    }

    public class Attributes
    {
        public Fix scale;
        public Fix speed;
        public Fix jumpStrength;
        public FixColor color;
        public Attributes Clone()
        {
            return new Attributes()
            {
                scale = scale,
                speed = speed,
                jumpStrength = jumpStrength,
                color = color
            };
        }
        public void CopyFrom(Attributes other)
        {
            scale = other.scale;
            speed = other.speed;
            jumpStrength = other.jumpStrength;
            color = other.color;
        }
    }

    public struct FixColor
    {
        public Fix r; public Fix g; public Fix b;

        public FixColor(Fix r, Fix g, Fix b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public void ToHSV(out Fix h, out Fix s, out Fix v)
        {
            // h, s, v = hue, saturation, value 
            Fix cmax = Fix.Max(r, Fix.Max(g, b)); // maximum of r, g, b 
            Fix cmin = Fix.Min(r, Fix.Min(g, b)); // minimum of r, g, b 
            Fix diff = cmax - cmin; // diff of cmax and cmin. 
            h = -Fix.One;
            s = -Fix.One;

            // if cmax and cmax are equal then h = 0 
            if (cmax == cmin)
                h = (Fix)0;

            // if cmax equal r then compute h 
            else if (cmax == r)
                h = (Fix.One / (Fix)6 * ((g - b) / diff) + Fix.One) % Fix.One;

            // if cmax equal g then compute h 
            else if (cmax == g)
                h = (Fix.One / (Fix)6 * ((b - r) / diff) + Fix.One / (Fix)3) % Fix.One;

            // if cmax equal b then compute h 
            else if (cmax == b)
                h = (Fix.One / (Fix)6 * ((r - g) / diff) + (Fix)2 / (Fix)3) % Fix.One;

            // if cmax equal zero 
            if (cmax == 0)
                s = Fix.Zero;
            else
                s = diff / cmax;

            // compute v 
            v = cmax;
        }

        public static FixColor FromHSV(Fix h, Fix s, Fix v)
        {
            Fix H = h;
            while (H < 0) { H += Fix.One; };
            while (H >= 1) { H -= Fix.One; };
            Fix R, G, B;
            if (v <= 0)
            { R = G = B = Fix.Zero; }
            else if (s <= 0)
            {
                R = G = B = v;
            }
            else
            {
                Fix hf = H * (Fix)6;
                int i = (int)Fix.Floor(hf);
                Fix f = hf - (Fix)i;
                Fix pv = v * (Fix.One - s);
                Fix qv = v * (Fix.One - s * f);
                Fix tv = v * (Fix.One - s * (Fix.One - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = v;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = v;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = v;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = v;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = v;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = v;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = v;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = v;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = v; // Just pretend its black/white
                        break;
                }
            }
            return new FixColor(Fix.Clamp01(R), Fix.Clamp01(G), Fix.Clamp01(B));
        }


        public Color ToColor()
        {
            return new Color((float)r, (float)g, (float)b);
        }
        public static implicit operator Color(FixColor fixColor) { return fixColor.ToColor(); }
    }

    public class Cow
    {
        public string name;
        public string description;
        public string purpose;
        public string speciesName;

        public Cow(string name, string description, string purpose)
        {
            speciesName = GetType().Name;
            this.name = name;
            this.description = description;
            this.purpose = purpose;
        }

        public string Moo()
        {
            return $"Moo, I am a {speciesName}. my name is {name} and I am described as {description} and my purpose is {purpose}.";
        }
    }
}
