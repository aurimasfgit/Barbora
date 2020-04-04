using Barbora.App.Utils;
using Barbora.Core.Models;
using System;
using System.IO;
using System.Media;

namespace Barbora.App.Services
{
    public interface ISoundPlayerService
    {
        void Play(SoundsEnum soundEnum);
    }

    public class SoundPlayerService : ISoundPlayerService
    {
        private string defaultSoundName = "Sounds.dixie-horn_daniel-simion.wav";

        private Stream GetSoundStream(SoundsEnum soundEnum)
        {
            var resourceName = string.Empty;

            switch (soundEnum)
            {
                case SoundsEnum.Default:
                    resourceName = defaultSoundName;
                    break;

                case SoundsEnum.BeepOne:
                    resourceName = "Sounds.beep-06.wav";
                    break;
                case SoundsEnum.CartoonBirds:
                    resourceName = "Sounds.cartoon-birds-2_daniel-simion.wav";
                    break;
                case SoundsEnum.CommonFart:
                    resourceName = "Sounds.Fart-Common-Everyday-Fart_Mike-Koenig.wav";
                    break;
                case SoundsEnum.DixieHorn:
                    resourceName = "Sounds.dixie-horn_daniel-simion.wav";
                    break;
                case SoundsEnum.HornHonk:
                    resourceName = "Sounds.Horn Honk-SoundBible.com-1162546405";
                    break;
                case SoundsEnum.SosMorseCode:
                    resourceName = "Sounds.sos-morse-code_daniel-simion.wav";
                    break;
                case SoundsEnum.Yes:
                    resourceName = "Sounds.yes-2.wav";
                    break;
                case SoundsEnum.Yummy:
                    resourceName = "yummy.wav";
                    break;

                default:
                    resourceName = defaultSoundName;
                    break;
            }

            return ResourceHelper.GetResourceStream(resourceName);
        }

        public void Play(SoundsEnum soundEnum)
        {
            var soundStream = GetSoundStream(soundEnum);

            if (soundStream == null)
                throw new ArgumentNullException("soundStream");

            SoundPlayer player = new SoundPlayer(soundStream);

            player.LoadAsync();
            player.Play();
            player.Dispose();
        }
    }
}