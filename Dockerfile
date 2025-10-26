# Dockerfile (수정 완료)

# 1. 베이스 이미지 설정
FROM python:3.10-slim

# 2. ffmpeg 설치 (오디오 처리를 위해 추가)
RUN apt-get update && apt-get install -y ffmpeg

# 3. Coqui TTS 라이선스 자동 동의 환경 변수 설정
ENV COQUI_TOS_AGREED=1

# 4. 작업 디렉토리 설정
WORKDIR /app

# 5. 필요한 파일 복사 및 라이브러리 설치
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# 6. 소스 코드 전체 복사
COPY . .

# 7. 앱 실행
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "7860"]