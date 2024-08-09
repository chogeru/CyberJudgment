using System.Linq;
using AbubuResouse.Log;
namespace AbubuResouse.Singleton
{
    /// <summary>
    /// SEの再生を管理するマネージャークラス
    /// </summary>
    public class SEManager : AudioManagerBase<SEManager>
    {
        /// <summary>
        /// データベース名として "se_data.db" を返す
        /// </summary>
        protected override string GetDatabaseName() => "se_data.db";

        /// <summary>
        /// 指定されたSE名と同じレコードをデータベースから検索して、SEを再生する
        /// </summary>
        /// <param name="bgmName">BGM名</param>
        /// <param name="volume">音量</param>
        public override void PlaySound(string clipName, float volume)
        {
            var query = connection.Table<SoundClip>().FirstOrDefault(x => x.ClipName == clipName);
            if (query != null)
            {
                LoadAndPlayClip($"SE/{query.ClipPath}", volume);
            }
            else
            {
                DebugUtility.Log($"指定されたサウンドクリップ名に一致するレコードがデータベースに存在しない: {clipName}");
            }
        }

        /// <summary>
        /// データベースのサウンドクリップテーブル
        /// </summary>
        private class SoundClip
        {
            public int Id { get; set; }
            public string ClipName { get; set; }
            public string ClipPath { get; set; }
        }
    }
}
