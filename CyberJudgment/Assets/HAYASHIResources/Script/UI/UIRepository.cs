using System;
using System.Collections.Generic;
using System.Linq;
using SQLite4Unity3d;

namespace AbubuResouse.MVP.Repository
{
    /// <summary>
    /// UIのボタンリンクデータベースを管理するリポジトリクラス
    /// </summary>
    public class UIRepository
    {
        private SQLiteConnection _connection;

        /// <summary>
        /// コンストラクタ
        /// データベース接続を初期化し、UIButtonLinkテーブルを作成
        /// </summary>
        /// <param name="databasePath">データベースファイルのパス</param>
        public UIRepository(string databasePath)
        {
            _connection = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
            _connection.CreateTable<UIButtonLink>();
        }

        /// <summary>
        /// 全てのボタンリンクのリストを取得
        /// </summary>
        /// <returns>すべてのUIButtonLinkオブジェクトのリスト</returns>
        public List<UIButtonLink> GetAllButtonLinks()
        {
            return _connection.Table<UIButtonLink>().ToList();
        }

        /// <summary>
        /// 指定されたボタン名に対応するボタンリンクを取得
        /// </summary>
        /// <param name="buttonName">ボタン名</param>
        /// <returns>対応するUIButtonLinkオブジェクト、見つからない場合はnull</returns>
        public UIButtonLink GetButtonLinkByName(string buttonName)
        {
            return _connection.Table<UIButtonLink>().FirstOrDefault(x => x.ButtonName == buttonName);
        }

        /// <summary>
        /// 新しいボタンリンクをデータベースに追加
        /// </summary>
        /// <param name="buttonLink">追加するUIButtonLinkオブジェクト</param>
        public void AddButtonLink(UIButtonLink buttonLink)
        {
            _connection.Insert(buttonLink);
        }
    }
}
