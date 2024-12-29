using System;
using System.Collections.Generic;
using System.Linq;
using SQLite4Unity3d;

namespace AbubuResouse.MVP.Repository
{
    /// <summary>
    /// UI�̃{�^�������N�f�[�^�x�[�X���Ǘ����郊�|�W�g���N���X
    /// </summary>
    public class UIRepository
    {
        private SQLiteConnection _connection;

        /// <summary>
        /// �R���X�g���N�^
        /// �f�[�^�x�[�X�ڑ������������AUIButtonLink�e�[�u�����쐬
        /// </summary>
        /// <param name="databasePath">�f�[�^�x�[�X�t�@�C���̃p�X</param>
        public UIRepository(string databasePath)
        {
            _connection = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
            _connection.CreateTable<UIButtonLink>();
        }

        /// <summary>
        /// �S�Ẵ{�^�������N�̃��X�g���擾
        /// </summary>
        /// <returns>���ׂĂ�UIButtonLink�I�u�W�F�N�g�̃��X�g</returns>
        public List<UIButtonLink> GetAllButtonLinks()
        {
            return _connection.Table<UIButtonLink>().ToList();
        }

        /// <summary>
        /// �w�肳�ꂽ�{�^�����ɑΉ�����{�^�������N���擾
        /// </summary>
        /// <param name="buttonName">�{�^����</param>
        /// <returns>�Ή�����UIButtonLink�I�u�W�F�N�g�A������Ȃ��ꍇ��null</returns>
        public UIButtonLink GetButtonLinkByName(string buttonName)
        {
            return _connection.Table<UIButtonLink>().FirstOrDefault(x => x.ButtonName == buttonName);
        }

        /// <summary>
        /// �V�����{�^�������N���f�[�^�x�[�X�ɒǉ�
        /// </summary>
        /// <param name="buttonLink">�ǉ�����UIButtonLink�I�u�W�F�N�g</param>
        public void AddButtonLink(UIButtonLink buttonLink)
        {
            _connection.Insert(buttonLink);
        }
    }
}
