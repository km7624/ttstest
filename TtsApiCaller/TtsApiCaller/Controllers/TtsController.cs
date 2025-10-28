// Controllers/TtsController.cs
using Microsoft.AspNetCore.Mvc;
using TtsApiCaller.Services;
using TtsApiCaller.Models; // 1. Models 네임스페이스를 추가합니다.

namespace TtsApiCaller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TtsController : ControllerBase
{
    private readonly TtsService _ttsService;

    public TtsController(TtsService ttsService)
    {
        _ttsService = ttsService;
    }

    [HttpPost("generate")]
    // 2. 여러 개로 받던 파라미터를 TtsRequest 모델 하나로 받도록 변경합니다.
    public async Task<IActionResult> GenerateSpeech([FromForm] TtsRequest request)
    {
        // 3. request 객체의 속성을 사용하여 값에 접근합니다.
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("텍스트를 입력해야 합니다.");
        }

        if (request.AudioFile == null || request.AudioFile.Length == 0)
        {
            return BadRequest("오디오 파일을 업로드해야 합니다.");
        }

        // 4. request 객체에서 파일 스트림과 이름을 꺼내 서비스에 전달합니다.
        var audioBytes = await _ttsService.GetSpeechAudioAsync(
            request.Text,
            request.AudioFile.OpenReadStream(),
            request.AudioFile.FileName
        );

        if (audioBytes != null && audioBytes.Length > 0)
        {
            return File(audioBytes, "audio/mpeg", "generated_speech.mp3");
        }

        return StatusCode(500, "허깅페이스 서버에서 음성을 생성하는 데 실패했습니다. 서버 로그를 확인하세요.");
    }
}