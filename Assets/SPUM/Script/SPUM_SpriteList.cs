using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SPUM_SpriteList : MonoBehaviour
{
    public List<SpriteRenderer> _itemList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _eyeList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _hairList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _bodyList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _clothList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _armorList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _pantList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _weaponList = new List<SpriteRenderer>();
    public List<SpriteRenderer> _backList = new List<SpriteRenderer>();

    public SPUM_HorseSpriteList _spHorseSPList;
    public string _spHorseString;
    // Start is called before the first frame update

    public Texture2D _bodyTexture;
    public string _bodyString;

    public List<string> _hairListString = new List<string>();
    public List<string> _clothListString = new List<string>();
    public List<string> _armorListString = new List<string>();
    public List<string> _pantListString = new List<string>();
    public List<string> _weaponListString = new List<string>();
    public List<string> _backListString = new List<string>();
    


    public void Reset()
    {
        for(var i = 0 ; i < _hairList.Count;i++)
        {
            if(_hairList[i]!=null) _hairList[i].sprite = null;
        }
        for(var i = 0 ; i < _clothList.Count;i++)
        {
            if(_clothList[i]!=null) _clothList[i].sprite = null;
        }
        for(var i = 0 ; i < _armorList.Count;i++)
        {
            if(_armorList[i]!=null) _armorList[i].sprite = null;
        }
        for(var i = 0 ; i < _pantList.Count;i++)
        {
            if(_pantList[i]!=null) _pantList[i].sprite = null;
        }
        for(var i = 0 ; i < _weaponList.Count;i++)
        {
            if(_weaponList[i]!=null) _weaponList[i].sprite = null;
        }
        for(var i = 0 ; i < _backList.Count;i++)
        {
            if(_backList[i]!=null) _backList[i].sprite = null;
        }
    }

    public void LoadSpriteSting()
    {

    }

    public void LoadSpriteStingProcess(List<SpriteRenderer> SpList, List<string> StringList)
    {
        for(var i = 0 ; i < StringList.Count ; i++)
        {
            if(StringList[i].Length > 1)
            {

                // Assets/SPUM/SPUM_Sprites/BodySource/Species/0_Human/Human_1.png
            }
        }
    }

    public void LoadSprite(SPUM_SpriteList data)
    {
        //스프라이트 데이터 연동
        SetSpriteList(_hairList,data._hairList);
        SetSpriteList(_bodyList,data._bodyList);
        SetSpriteList(_clothList,data._clothList);
        SetSpriteList(_armorList,data._armorList);
        SetSpriteList(_pantList,data._pantList);
        SetSpriteList(_weaponList,data._weaponList);
        SetSpriteList(_backList,data._backList);
        SetSpriteList(_eyeList,data._eyeList);
        
        if(data._spHorseSPList!=null)
        {
            SetSpriteList(_spHorseSPList._spList,data._spHorseSPList._spList);
            _spHorseSPList = data._spHorseSPList;
        }
        else
        {
            _spHorseSPList = null;
        }

        //색 데이터 연동.
        if(_eyeList.Count> 2 &&  data._eyeList.Count > 2 )
        {
            _eyeList[2].color = data._eyeList[2].color;
            _eyeList[3].color = data._eyeList[3].color;
        }

        _hairList[3].color = data._hairList[3].color;
        _hairList[0].color = data._hairList[0].color;
        //꺼져있는 오브젝트 데이터 연동.x
        _hairList[0].gameObject.SetActive(!data._hairList[0].gameObject.activeInHierarchy);
        _hairList[3].gameObject.SetActive(!data._hairList[3].gameObject.activeInHierarchy);

        _hairListString = data._hairListString;
        _clothListString = data._clothListString;
        _pantListString = data._pantListString;
        _armorListString = data._armorListString;
        _weaponListString = data._weaponListString;
        _backListString = data._backListString;
    }

    public void SetSpriteList(List<SpriteRenderer> tList, List<SpriteRenderer> tData)
    {
        for(var i = 0 ; i < tData.Count;i++)
        {
            if(tData[i]!=null) 
            {
                tList[i].sprite = tData[i].sprite;
                tList[i].color = tData[i].color;
            }
            else tList[i] = null;
        }
    }

    public void ResyncData()
    {
        SyncPath(_hairList,_hairListString);
        SyncPath(_clothList,_clothListString);
        SyncPath(_armorList,_armorListString);
        SyncPath(_pantList,_pantListString);
        SyncPath(_weaponList,_weaponListString);
        SyncPath(_backList,_backListString);
    }

    public void SyncPath(List<SpriteRenderer> _objList, List<string> _pathList)
    {
        for (var i = 0; i < _pathList.Count; i++)
        {
            if (_pathList[i].Length > 1)
            {
                string tPath = _pathList[i];
                tPath = tPath.Replace("Assets/Resources/", "");
                tPath = tPath.Replace(".png", "");

                Sprite[] tSP = Resources.LoadAll<Sprite>(tPath);
                if (tSP.Length > 1)
                {
                    _objList[i].sprite = tSP[i];
                }
                else if (tSP.Length > 0)
                {
                    _objList[i].sprite = tSP[0];
                }
            }
            else
            {
                _objList[i].sprite = null;
            }
        }
    }

    public void CopyDataFromBase()
    {
        int idx;
        if (int.TryParse(transform.parent.parent.gameObject.name.Substring(8), out idx))
        {
            SPUM_Prefabs _baseData = Resources.Load<SPUM_Prefabs>(string.Format("SPUM/SPUM_Units/Unit{0:D3}", idx));
            if (_baseData == null) Debug.LogError(string.Format("SPUM named Unit{0:D3} does not exist in Assets/Resources/SPUM/SPUM_Units", idx));
            else
            {
                SyncSprites(_itemList, _baseData._spriteOBj._itemList);
                SyncSprites(_eyeList, _baseData._spriteOBj._eyeList);
                SyncSprites(_hairList, _baseData._spriteOBj._hairList);
                SyncSprites(_bodyList, _baseData._spriteOBj._bodyList);
                SyncSprites(_clothList, _baseData._spriteOBj._clothList);
                SyncSprites(_armorList, _baseData._spriteOBj._armorList);
                SyncSprites(_pantList, _baseData._spriteOBj._pantList);
                SyncSprites(_weaponList, _baseData._spriteOBj._weaponList);
                SyncSprites(_backList, _baseData._spriteOBj._backList);
            }
        }
        else Debug.LogError("Wrong Name Format: must be [Monster N]");
    }

    private void SyncSprites(List<SpriteRenderer> _target, List<SpriteRenderer> _base)
    {
        for (int i = _base.Count - 1; i >= 0; i--)
        {
            _target[i].sprite = _base[i].sprite;
            _target[i].color = _base[i].color;
        }
    }
}
