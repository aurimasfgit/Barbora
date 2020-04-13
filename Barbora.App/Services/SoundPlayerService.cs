using Barbora.App.Models;
using Barbora.App.Helpers;
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
        private readonly string defaultSoundName = "Sounds.dixie-horn_daniel-simion.wav";

        private Stream GetSoundStream(SoundsEnum soundEnum)
        {
            string resourceName = soundEnum switch
            {
                SoundsEnum.Default => defaultSoundName,
                SoundsEnum.BeepOne => "Sounds.beep-06.wav",
                SoundsEnum.CartoonBirds => "Sounds.cartoon-birds-2_daniel-simion.wav",
                SoundsEnum.CommonFart => "Sounds.Fart-Common-Everyday-Fart_Mike-Koenig.wav",
                SoundsEnum.DixieHorn => "Sounds.dixie-horn_daniel-simion.wav",
                SoundsEnum.HornHonk => "Sounds.Horn Honk-SoundBible.com-1162546405",
                SoundsEnum.SosMorseCode => "Sounds.sos-morse-code_daniel-simion.wav",
                SoundsEnum.Yes => "Sounds.yes-2.wav",
                SoundsEnum.Yummy => "Sounds.yummy.wav",
                _ => defaultSoundName,
            };

            return ResourceHelper.GetResourceStream(resourceName);
        }

        public void Play(SoundsEnum soundEnum)
        {
            var soundStream = GetSoundStream(soundEnum);

            if (soundStream == null)
                throw new ArgumentNullException("soundStream");

            var player = new SoundPlayer(soundStream);

            player.LoadAsync();
            player.Play();
            player.Dispose();
        }
    }
}