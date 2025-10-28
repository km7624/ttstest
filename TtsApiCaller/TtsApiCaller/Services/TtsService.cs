// Services/TtsService.cs
using System.Net.Http.Headers;

namespace TtsApiCaller.Services;

public class TtsService
{
    private readonly HttpClient _httpClient;

    // Program.cs에서 등록한 HttpClient 인스턴스를 자동으로 받아옵니다.
    public TtsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// 허깅페이스 TTS API를 호출하여 음성 데이터를 받아옵니다.
    /// </summary>
    /// <param name="textToSpeak">음성으로 변환할 텍스트</param>
    /// <param name="audioFileStream">목소리 샘플 오디오 파일의 스트림</param>
    /// <param name="fileName">오디오 파일의 원본 파일명</param>
    /// <returns>생성된 MP3 오디오 데이터 (byte array) 또는 실패 시 null</returns>
    public async Task<byte[]?> GetSpeechAudioAsync(string textToSpeak, Stream audioFileStream, string fileName)
    {
        // main.py에서 정의한 API 엔드포인트 경로
        var apiEndpoint = "/generate-speech-api/";

        // 파일을 전송하기 위해 MultipartFormDataContent를 사용합니다.
        using var content = new MultipartFormDataContent();

        // 1. 텍스트 데이터 추가 (Python API의 'text' 파라미터와 이름이 같아야 함)
        content.Add(new StringContent(textToSpeak), "text");

        // 2. 오디오 파일 데이터 추가 (Python API의 'speaker_wav' 파라미터와 이름이 같아야 함)
        var streamContent = new StreamContent(audioFileStream);
        // 파일의 Content-Type을 지정해주는 것이 좋습니다.
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(streamContent, "speaker_wav", fileName);

        try
        {
            // POST 요청을 보냅니다.
            HttpResponseMessage response = await _httpClient.PostAsync(apiEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                // 성공하면, MP3 오디오 데이터를 바이트 배열로 읽어와 반환합니다.
                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                // API 호출 실패 시, 서버 로그에 자세한 에러를 출력합니다.
                string errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[TtsService] API Error: {response.StatusCode}, Body: {errorBody}");
                return null;
            }
        }
        catch (Exception ex)
        {
            // 네트워크 오류 등 예외가 발생하면 로그를 출력합니다.
            Console.WriteLine($"[TtsService] Exception during API call: {ex.Message}");
            return null;
        }
    }
}