
using System;
using System.Collections;
using System.Collections.Generic;

namespace WebConnector {
    public class ErrorResource {
        public ErrorCode code { get; set; }
        public string val { get; set; }
        public string comment { get; set; }
    }

    public enum ErrorCode {
        NETWORK_ERROR, //네트워크 오류
        RESPONSE_PARSING_ERROR, //응답값 파싱 오류

        //From 서버
        NOT_LOGIN,//로그인되어 있지 않음
        NOT_EXIST_USER, //존재하지 않는 유저
        ALREADY_USED_TEAMNAME, //이미 존재하는 팀명
        LACK_RUBY, //루비 부족
        LACK_GOLD, //골드 부족
        LACK_FRIENDPOINT, //우정포인트 부족
        LACK_LVPCOIN, //코인 부족
        LACK_PLAYBALL, // 플레이볼 부족
        LACK_TICKET, //타켓 부족
        CARD_SLOT_LIMIT_EXCEEDED, //카드 슬롯 제한 초과. (val : "보유카드수,현재슬롯수")
        CANNOT_SEARCH_TEAM, //팀을 찾을 수 없음
        NOT_GOLD_EXCHANGEABLE_TIME, //골드교환 가능시간이 아님
        ALREADY_HAS_COMMUTER_GOODS, //이미 월정액 상품 보유중임

        LACK_ITEM, //보유아이템 부족

        UNSALABLE_ITEM, //판매불가 아이템

        INVALID_USER_CARD, //유효하지 않은 선수카드
        CARD_IS_LOCKED, //잠긴 카드
        LACK_SKIP_TICKET, //스킵 티켓 부족

        //팀관리
        CANNOT_SAME_PLAYER_IN_MAJOR, //1군에 같은 선수 세팅 불가
        CANNOT_CHANGE_MIS_POSITION, //포지션 미스매치
        CANNOT_CHANGE_DIFFERENT_TYPE, //다른 선수타입 교체불가
        CANNOT_CHANGE_MINORS, //2군끼리 교체불가
        CANNOT_CHANGE_BENCHES, //벤치끼리 교체불가
        CANNOT_CHANGE_SUSPENDED_STARTER, //출장정지중인 선발 교체불가

        [Obsolete("삭제됨")]
        CANNOT_MOUNT_SETDECK, //장착불가 세트덱

        WEEKLY_SEASON_DOES_NOT_START, //주간 시즌이 시작되지 않음.

        //랭킹전
        RANKEDPLAY_SEASON_CLOSED, // 랭킹전 시즌 종료됨
        RANKEDPLAY_CANNOT_FIND_OTHER, //랭킹전 상대팀 검색 실패
        RANKEDPLAY_INVALID_REVENGE_HISTORY, //리벤트 가능 전적이 아님
        //쟁탈전
        RACEPLAY_DAILY_LEAGUE_EXPIRED, //쟁탈전 일일 리그 종료됨

        // 상점
        INVALID_GOODS, //유효하지 않은 상품
        FINISH_SALE_GOODS, //판매 종료된 아이템
        CANNOT_USE_COUPON, //사용 불가능 쿠폰

        //친구
        FRIEND_REQ_DAILY_LIMIT_EXCEEDED, //일일 친구요청 제한 초과
        FRIEND_LIMIT_EXCEEDED, //친구수 제한 초과
        FRIEND_WAITING_LIST_LIMIT_EXCEEDED, //친구 대기수 제한 초과
        FRIEND_DEL_DAILY_LIMIT_EXCEEDED, //일일 친구삭제 제한 초과
        FRIEND_POINT_LIMIT_EXCEEDED, //우정포인트 보유량 초과
        FRIEND_OTHER_LIMIT_EXCEEDED, //친구수락시 상대방의 친구수 제한 초과

        MISSION_INCOMPLETED, //완료되지 않은 미션
        MISSION_ALREADY_REWARDED, //이미 리워드 받은 미션
        MISSION_NOT_ACHIEVE, //미션 미달성

        TRAINING_DOESNOT_FINISH, //특별훈련 미종료

        NOT_ALLOW_VERSION, //클라이언트 버전업 필요

        RACEPLAY_LACK_OF_TICKET, //쟁탈전 재경기 티켓 부족

        WALKOFFPLAY_TEAM_LEVEL_LIMIT, //9회말2아웃 팀레벨 제한 조건
        WALKOFFPLAY_DAILY_LIMIT_EXCEEDED, //9회말2아웃 일일 최대 경기수 초과
        WALKOFFPLAY_DAILY_HITTER_LIMIT, //9회말2아웃 타자 일일 1회 사용 제한

        SCOUT_PICKUP_TIMEOUT, //스카웃 비복원 가차 유효시간 초과
        LVP_COIN_STORE_TIMEOUT, //라이브매치 코인스토어 유효시간 초과

        INVALID_ACCESS, //잘못된 요청
        SYSTEM_ERROR //시스템 오류
    }
}