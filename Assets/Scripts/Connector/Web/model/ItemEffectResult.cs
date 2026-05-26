using System;
using System.Collections.Generic;
using Utils;

namespace WebConnector
{
    /// <summary>
    /// 아이템 지급 결과
    /// </summary>
    public class ItemEffectResult {
        public int itemId { get; set; }

        //결과 Object
        private string _result;
        //선수팩을 받은 경우 결과 Object
        private List<GameCardInfo> _cardInfos;

        public string result { set { _result = value; } }

        /// <summary>
        /// 지급된 아이템 타입
        /// </summary>
        public ItemType GetItemType {
            get { return ItemUtils.detectItemTypeFrom(itemId); }
        }
        
        /// <summary>
        /// 선수카드팩 이면 받은 카드 정보
        /// return : 카드 정보 리스트
        /// </summary>
        public List<GameCardInfo> GetResultIfCardPack
        {
            get
            {
                ItemType type = GetItemType;
                if (type == ItemType.CardPack || type == ItemType.CardPackRandom || type == ItemType.CardPackTeam || type == ItemType.CardPackTeamYear)
                {
                    return JsonUtils.Deserialize<List<GameCardInfo>>(_result.Trim());
                }
                return null;
            }
        }
        /// <summary>
        /// 장비팩 이면 지급받은 장비 정보
        /// </summary>
        public List<GearInfo> GetResultIfGearPack
        {
            get {
                ItemType type = GetItemType;
                if (type == ItemType.GearPack) {
                    return JsonUtils.Deserialize<List<GearInfo>>(_result.Trim());
                }
                return null;
            }
        }
    }
}