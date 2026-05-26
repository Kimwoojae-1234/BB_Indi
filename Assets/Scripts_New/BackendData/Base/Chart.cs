// Copyright 2013-2022 AFI, INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;
using BackEnd;
using LitJson;
using UnityEngine;

namespace BackendData.Base {
    //===============================================================
    // 차트 불러오기에 대한 공통적인 로직을 가진 클래스
    //===============================================================
    public abstract class Chart : Normal {

        // Backend.Chart.GetChartContents 호출 시 리턴되는 json(Flatten)을 처리하는 함수
        // 각 자식 객체에서 형태에 맞게 설정해야한다.
        protected abstract void LoadChartDataTemplate(JsonData json);
        public  abstract string GetChartFileName(); // 각 자식 객체가 설정한 차트 이름을 불러오는 함수
    
        //차트에 등록된 이미지 경로를 찾아 이미지를 반환하는 함수
        protected Sprite AddOrGetImageDictionary(Dictionary<string, Sprite> imageDictionary, string imagePath, string imageName) {
            if (imageDictionary.ContainsKey(imageName)) {
                return imageDictionary[imageName];
            }

            imagePath += imageName;
            var sprite = Resources.Load<Sprite>(imagePath);

            if (sprite == null) {
                Debug.LogWarning("이미지를 찾지 못했습니다. in " + imageName);
                return null;
            }
            imageDictionary.Add(imageName, sprite);
            return imageDictionary[imageName];
        }
    
        // Base가 부모인 객체에서 공통적으로 사용되는 뒤끝 차트 로딩 함수
        // 차트 이름에 맞는 함수를 로드하고 LoadChartDataTemplate로 각자의 객체에서 사용 가능하게끔 파싱해준다.
        public void BackendChartDataLoad(AfterBackendLoadFunc afterBackendLoadFunc) {

            string chartFileName = GetChartFileName();
            string className = GetType().Name;
            bool isSuccess = false;
            string errorInfo = string.Empty;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            string chartId = string.Empty;

            if (!KOBManager.Backend.Chart.ChartInfo.Dictionary.ContainsKey(chartFileName)) {
                afterBackendLoadFunc(isSuccess, className, funcName, $"차트에 {chartFileName}에 대한 정보가 존재하지 않습니다.");
                return;
            }

            chartId = KOBManager.Backend.Chart.ChartInfo.Dictionary[chartFileName];

            // [뒤끝] 차트 불러오기 함수
            SendQueue.Enqueue(BackEnd.Backend.Chart.GetChartContents, chartId, callback => {
                try {
                    Debug.Log($"Backend.Chart.GetChartContents({chartId}) : {callback}");

                    if (callback.IsSuccess()) {
                        JsonData json = callback.FlattenRows();

                        LoadChartDataTemplate(json); // 각 자식 객체가 설정한 리턴값 파싱 함수
                    
                        isSuccess = true;
                    }
                    else {
                        errorInfo = callback.ToString();
                    }
                }
                catch (Exception e) {
                    errorInfo = e.ToString();
                }
                finally {
                    afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
                }
            });
        }



        public void BackendChartAndSave(AfterBackendLoadFunc afterBackendLoadFunc)
        {
            string chartFileName = GetChartFileName();
            string className = GetType().Name;
            bool isSuccess = false;
            string errorInfo = string.Empty;
            string funcName = MethodBase.GetCurrentMethod()?.Name;

            string chartId = string.Empty;

            if (!KOBManager.Backend.Chart.ChartInfo.Dictionary.ContainsKey(chartFileName))
            {
                afterBackendLoadFunc(isSuccess, className, funcName, $"차트에 {chartFileName}에 대한 정보가 존재하지 않습니다.");
                return;
            }

            chartId = KOBManager.Backend.Chart.ChartInfo.Dictionary[chartFileName];            
            var broString = Backend.Chart.GetLocalChartData(chartId); //로컬 데이터
            bool bDownLoad = false;
            if (string.IsNullOrEmpty(broString)) //로컬 차트 없는 경우
            {
                //로컬에 없는 경우 다운
                bDownLoad = true;
            }
            else
            {
                if (KOBManager.Backend.ChartLocalVersion.ContainsKey(chartFileName) == false)
                {
                    //로컬차트 저장 정보가 없는 경우 
                    bDownLoad = true;
                }                
                else
                {
                    //정상케이스 - 서버 정보와 로컬에 저장된 정보의 버전을 비교
                    string LocalID = KOBManager.Backend.ChartLocalVersion[chartFileName];
                    if(chartId != LocalID)
                    {
                        //버전이 다르면 다운
                        bDownLoad = true;
                    }
                }

                if(bDownLoad == true)
                {
                    //재다운로드시 해당 차트 아이디 삭제
                    try
                    {
                        Backend.Chart.DeleteLocalChartData(chartId);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("로컬 차트 삭제 실패 e : " + e);
                    }
                }
            }

            if(bDownLoad == true)
            {
                Debug.Log("차트 새로 다운 받음 // 차트 이름 : " + chartFileName);
                KOBManager.Backend.UpdateChartVersion(chartFileName, chartId); //서버로 부터 다운받을 경우 로컬 정보를 업데이트 해중다
                //서버로부터 저장
                SendQueue.Enqueue(BackEnd.Backend.Chart.GetOneChartAndSave, chartId, callback => {
                    try
                    {
                        Debug.Log($"Backend.Chart.GetOneChartAndSave({chartId}) : {callback}");

                        if (callback.IsSuccess())
                        {
                            JsonData json = callback.FlattenRows();
                            LoadChartDataTemplate(json); // 각 자식 객체가 설정한 리턴값 파싱 함수
                            isSuccess = true;
                        }
                        else
                        {
                            errorInfo = callback.ToString();
                        }
                    }
                    catch (Exception e)
                    {
                        errorInfo = e.ToString();
                    }
                    finally
                    {
                        afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
                    }
                });
            }
            else
            {
                //서버와 통신없이 로컬 데이터를 사용하는 경우
                Debug.Log("로컬차트를 사용 // 차트 이름 : " + chartFileName);
                try
                {
                    //로컬 데이터 불러오기
                    JsonData chartJson = BackendReturnObject.Flatten(JsonMapper.ToObject(broString))["rows"];
                    LoadChartDataTemplate(chartJson); // 각 자식 객체가 설정한 리턴값 파싱 함수
                    isSuccess = true;
                }
                catch (Exception e)
                {
                    errorInfo = e.ToString();
                }
                finally
                {
                    afterBackendLoadFunc(isSuccess, className, funcName, errorInfo);
                }
            }
        }

    }





















}


/*
public class ChartInfo
{
    public string chartName;
    public string chartFileId;
    public string updateDate;

    public ChartInfo(JsonData json)
    {
        chartName = json["chartName"].ToString();
        chartFileId = json["chartFileId"].ToString();
        updateDate = json["updateDate"].ToString();
    }

    private void AAA(BackendReturnObject serverChartBro)
    {
        // 서버에서 불러온 ChartManager을 언마샬하여 JsonData 형태로 캐싱합니다.
        JsonData newChartManagerJson = serverChartBro.FlattenRows();

        // 차트 이름으로 데이터를 검색할 것이기 때문에 Dictnary로 생성합니다.
        // 해당 Dictnary는 최신 버전으로 업데이트할 차트 리스트로 사용됩니다.(최신 버전이라면 해당 리스트에서 제외)
        Dictionary<string, ChartInfo> chartInfoDic = new Dictionary<string, ChartInfo>();

        // 반복문을 통해 모든 ChartManager 차트에 저장된 모든 차트들을 삽입합니다.
        foreach (JsonData chartInfoJson in newChartManagerJson)
        {
            ChartInfo chartInfo = new ChartInfo(chartInfoJson);
            chartInfoDic.Add(chartInfo.chartName, chartInfo);
        }


        if (string.IsNullOrEmpty(deviceChartManagerString) == false)
        {
            // 기기에 저장된 chartManager 차트가 존재한다면

            // 기기에 저장된 string형태의 chartManager를 Json 형태로 변경
            JsonData deviceChartManagerJson = JsonMapper.ToObject(deviceChartManagerString);
            deviceChartManagerJson = BackendReturnObject.Flatten(deviceChartManagerJson);

            // 기기에 저장된 chartManager 차트 속 차트들을 서버에서 불러온 데이터와 대조합니다.
            foreach (JsonData deviceChartJson in deviceChartManagerJson["rows"])
            {
                ChartInfo deviceChartInfo = new ChartInfo(deviceChartJson);

                // 이미 기기에 저장되어 있는 차트가 있는지 확인합니다.
                if (chartInfoDic.ContainsKey(deviceChartInfo.chartName))
                {

                    // 기기에 저장되어 있는 차트의 수정 날짜(updateDate)가 일치하는지 확인합니다.
                    if (chartInfoDic[deviceChartInfo.chartName].updateDate == deviceChartInfo.updateDate)
                    {
                        // 수정날짜까지 일치할 경우, 재다운로드 리스트(chartInfoDic)에서 제외합니다.
                        chartInfoDic.Remove(deviceChartInfo.chartName);
                    }
                }
            }
        }


        // 재다운로드할 차트 리스트에서 차트가 하나라도 존재하는지 확인합니다.
        if (chartInfoDic.Count > 0)
        {

            // 차트를 재다운로드하여 기기에 덮어씌웁니다.
            foreach (var downloadChartInfo in chartInfoDic)
            {
                Debug.Log(downloadChartInfo.Value.chartName + "을 새로운 버전으로 다운받습니다.");
                Backend.Chart.GetOneChartAndSave(downloadChartInfo.Value.chartFileId, downloadChartInfo.Value.chartName);
            }

            // chartManager 차트를 최신화합니다.
            Backend.Chart.GetOneChartAndSave(chartManagerFileId, chartManagerName);
        }
        else
        {
            Debug.Log("업데이트할 내역이 존재하지 않습니다.");
        }
    }
}*/


