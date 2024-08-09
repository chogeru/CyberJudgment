using System.Linq;
using AbubuResouse.Log;

namespace AbubuResouse.Singleton
{
    /// <summary>
    /// ボイスの再生を管理するマネージャークラス
    /// </summary>
    public class VoiceManager : AudioManagerBase<VoiceManager>
    {
        /// <summary>
        /// データベース名として "voice_data.db" を返す
        /// </summary>
        protected override string GetDatabaseName() => "voice_data.db";

        /// <summary>
        /// 指定されたVoiceClip名と同じレコードをデータベースから検索して、VoiceSEを再生する
        /// </summary>
        /// <param name="bgmName">BGM名</param>
        /// <param name="volume">音量</param>
        public override void PlaySound(string clipName, float volume)
        {
            var query = connection.Table<VoiceClip>().FirstOrDefault(x => x.ClipName == clipName);
            if (query != null)
            {
                LoadAndPlayClip($"Voice/{query.ClipPath}", volume);
            }
            else
            {
                DebugUtility.Log($"指定されたサウンドクリップ名に一致するレコードがデータベースに存在しない: {clipName}");
            }
        }

        /// <summary>
        /// データベースのボイスクリップテーブル
        /// </summary>
        private class VoiceClip
        {
            public int Id { get; set; }
            public string ClipName { get; set; }
            public string ClipPath { get; set; }
        }
    }
}
