using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebConnector {
    /// <summary>
    /// 리소스 버전 정보
    /// </summary>
    public class ResourceVersion {
        /// <summary>
        /// 현재 헤드 버전
        /// </summary>
        public string ver { get; set; }
        /// <summary>
        /// 리소스파일 다운로드 url (null 이면 패치 필요 없음)
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// 파일명(확장자명 제외)
        /// </summary>
        public string file { get; set; }
        /// <summary>
        /// 파일조각 갯수
        /// </summary>
        public int cnt { get; set; }
        /// <summary>
        /// 다운받을 파일이 전체 파일인지 여부 (true 이면 전체파일이므로 기존 폴더를 모두 지워도 상관없다)
        /// </summary>
        public bool IsWholeFile {
            get {
                if (!string.IsNullOrEmpty(file)) {
                    string[] arrFile = file.Split('-');
                    if (arrFile != null && arrFile.Length > 1 && arrFile[1] == "0") {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
