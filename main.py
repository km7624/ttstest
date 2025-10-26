# coding: utf-8
import os
import io
import shutil
import tempfile
import torch
from fastapi import FastAPI, HTTPException, File, UploadFile, Request, Form
from fastapi.responses import FileResponse, StreamingResponse, HTMLResponse
from fastapi.templating import Jinja2Templates
from fastapi.staticfiles import StaticFiles
from pydub import AudioSegment
from TTS.api import TTS

app = FastAPI()

try:
    templates_dir = os.path.join(os.path.dirname(__file__), "templates")
    templates = Jinja2Templates(directory=templates_dir)
except Exception as ex:
    print(f"⚠️ WARNING: Failed to initialize Jinja2Templates. Error: {ex}")

print("🚀 Loading XTTS-v2 model...")
try:
    device = "cuda" if torch.cuda.is_available() else "cpu"
    tts = TTS("tts_models/multilingual/multi-dataset/xtts_v2").to(device)
    print(f"✅ Model loaded successfully on {device}.")
except Exception as ex:
    print(f"❌ Failed to load TTS model. Error: {ex}")
    tts = None

@app.get("/", response_class=HTMLResponse)
async def serve_frontend(request: Request):
    return templates.TemplateResponse("index.html", {"request": request})

@app.post("/generate-speech-api/")
async def generate_speech_api(
    text: str = Form(...),
    speaker_wav: UploadFile = File(...)
):
    if tts is None:
        raise HTTPException(status_code=500, detail="TTS 모델이 로드되지 않았습니다.")
    
    temp_input_path = None
    temp_wav_path = None
    output_path = None
    
    try:
        # 출력 디렉토리 생성
        output_dir = "generated_audio"
        os.makedirs(output_dir, exist_ok=True)
        
        # 오디오 파일 임시 저장
        with tempfile.NamedTemporaryFile(delete=False, suffix=".wav") as temp_input_file:
            content = await speaker_wav.read()
            temp_input_file.write(content)
            temp_input_path = temp_input_file.name
        
        print(f"📥 Uploaded file saved to: {temp_input_path}")
        
        # 업로드된 오디오를 WAV로 변환
        sound = AudioSegment.from_file(temp_input_path)
        
        with tempfile.NamedTemporaryFile(delete=False, suffix=".wav") as temp_wav_file:
            temp_wav_path = temp_wav_file.name
        
        sound.export(temp_wav_path, format="wav")
        print(f"🔄 Converted WAV file saved to: {temp_wav_path}")
        
        # XTTS로 음성 생성 (파일로 직접 저장)
        print(f"🗣️ Generating speech with XTTS...")
        
        import numpy as np
        from datetime import datetime
        
        # 타임스탬프로 고유 파일명 생성
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        output_filename = f"output_{timestamp}.wav"
        output_path = os.path.join(output_dir, output_filename)
        
        # TTS 실행 - 파일로 직접 저장
        tts.tts_to_file(
            text=text,
            speaker_wav=temp_wav_path,
            language="ko",
            file_path=output_path
        )
        
        print(f"✅ Speech generated successfully: {output_path}")
        
        # WAV를 MP3로 변환
        mp3_filename = f"output_{timestamp}.mp3"
        mp3_path = os.path.join(output_dir, mp3_filename)
        
        sound = AudioSegment.from_wav(output_path)
        sound.export(mp3_path, format="mp3")
        
        print(f"🎵 MP3 file saved: {mp3_path}")
        
        # MP3 파일을 스트리밍으로 반환
        with open(mp3_path, 'rb') as f:
            audio_data = f.read()
        
        return StreamingResponse(
            io.BytesIO(audio_data), 
            media_type="audio/mpeg",
            headers={
                "Content-Disposition": f"inline; filename={mp3_filename}"
            }
        )
    
    except Exception as e:
        error_msg = f"음성 생성 중 오류 발생: {str(e)}"
        print(f"❌ {error_msg}")
        import traceback
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=error_msg)
    
    finally:
        # 임시 파일만 정리 (생성된 파일은 보존)
        if temp_input_path and os.path.exists(temp_input_path):
            os.remove(temp_input_path)
            print(f"🧹 Cleaned up input file: {temp_input_path}")
        if temp_wav_path and os.path.exists(temp_wav_path):
            os.remove(temp_wav_path)
            print(f"🧹 Cleaned up wav file: {temp_wav_path}")