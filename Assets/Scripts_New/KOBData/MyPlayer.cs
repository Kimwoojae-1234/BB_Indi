using System.Collections;
using System.Collections.Generic;

public class MyPlayer
{
    //포지션
    public int position;
    //타순
    public int order;
    //필요없을 것 같음
    public int cardDataIndex;
    //필요없을 것 같음
    public int level;
    //필요없을 것 같음
    public int cardType;
    //바디타입
    public int BodyInType;
    //컨택레벨 -> 레벨로부터 능력치 환산
    public int contactLevel;
    //컨택훈련포인트 -> 포인트로부터 레벨 산출
    public int contactPoint;
    //파워레벨
    public int powerLevel;
    //파워훈련포인트
    public int powerPoint;
    //선구레벨
    public int visionLevel;
    //선구훈련포인트
    public int visionPoint;
    //수비레벨
    public int fieldingLevel;
    //수비훈련포인트
    public int fieldingPoint;
    //송구레벨
    public int throwingLevel;
    //송구훈련포인트
    public int throwingPoint;
    //주력레벨
    public int speedLevel;
    //주력훈련포인트
    public int speedPoint;
    //제구레벨 -> 투수능력은 Two-Way 시 필요!!
    public int controlLevel;
    //제구훈련포인트
    public int controlPoint;
    //직구레벨
    public int fastballLevel;
    //직구훈련포인트
    public int fastballPoint;
    //커브레벨
    public int curveLevel;
    //커브훈련포인트
    public int curvePoint;
    //슬라이더레벨
    public int sliderLevel;
    //슬라이더훈련포인트
    public int sliderPoint;
    //싱커레벨
    public int sinkerLevel;
    //싱커훈련포인트
    public int sinkerPoint;
    //체인지업레벨
    public int changeupLevel;
    //체인지업훈련포인트
    public int changeupPoint;
    //보유스킬 리스트 -> 사용가능한 스킬 리스트
    public List<int> skillCollectionList;
    //보유스킬 리스트 -> 현재 장착한 스킬
    public List<int> skillEquipList;
    //스킬 슬롯수 -> 스킬 슬롯
    public int skillSlot;
    //보유 아이템 -> 보유 아이템 (PVP시 동료들과같이 쓸수 있음)
    public List<int> batItemList;
    //현재 장착 배트
    public int batEquipNumber;
}
