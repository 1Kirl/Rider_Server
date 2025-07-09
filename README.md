# 🚗 NeonServer_LiteNetLib

실시간 멀티플레이 자동차 게임을 위해 설계된 **LiteNetLib 기반 UDP 서버**입니다.

**Bit 단위 패킷 전송,**

**Domain-Driven Design(DDD),**

**Event-Driven Architecture(EDA)**
를 적용하여

빠르고 구조적으로 유연한 네트워크 서버를 목표로 설계되었습니다.

---

## 🧱 기술 스택

| 항목 | 내용 |
| --- | --- |
| 언어 | C# (.NET 8.0) |
| 네트워크 라이브러리 | [LiteNetLib](https://github.com/RevenantX/LiteNetLib) |
| 아키텍처 패턴 | Layered Architecture + Domain-Driven Design |
| 통신 최적화 | 비트 단위 패킷 압축 (BitWriter / BitReader) |
| 배포 환경 | Ubuntu 22.04 (Vultr VPS), `.NET publish`, `tmux` 유지 실행 |

---

## 🚀 주요 기능

### ✅ 비트 단위 패킷 전송

- `BitWriter` / `BitReader`를 직접 구현하여 입력 및 상태 패킷을 **3~7비트 단위**로 압축
- JSON 방식 대비 평균 **5~20배** 이상 전송 크기 감소

### ✅ 이벤트 기반 게임 흐름 (Event-Driven Architecture)

- `GameSession` 내부 이벤트: `OnGameStart`, `OnPlayerInputReceived`, `OnGameEnded` 등
- `NetworkManager`가 해당 이벤트를 구독하여 **메시지 전송과 중계 책임 분리**

### ✅ 도메인 중심 객체 설계 (DDD)

- `Player`, `GameSession`, `Matchmaker`는 **게임 개념에 따른 책임 분리**
- 도메인 상태(점수, 도착 여부, 입력 등)는 각 객체가 자체적으로 보유

### ✅ 인메모리 세션/유저 상태 관리

- 모든 세션과 유저 상태는 서버 메모리 내 Dictionary로 관리
- 외부 DB 없이도 빠른 응답 가능
- 닉네임, 커스터마이징 정보 등은 GSSAS(뒤끝) API를 통해 별도 관리

---

## 🧪 실행 방법


```bash
git clone <https://github.com/yourusername/NeonServer_LiteNetLib.git>
cd NeonServer_LiteNetLib
dotnet build -c Release
dotnet run --project ./NeonServer_LiteNetLib.csproj
```

서버 기본 포트: 7777


단일 인스턴스 기준으로 동작합니다 (Vultr Ubuntu VPS 기반 배포 테스트 완료)


### 🔍 개발 노트
이 프로젝트는 대부분 1인 개발 체제를 전제로 전체 서버 구조를 설계하고 구현한 프로젝트입니다.

따라서 GitHub 상의 커밋 메시지, PR, 협업 히스토리는 많지 않지만,

전체 로직 설계, 네트워크 흐름, 최적화 기법, 배포까지 모든 과정을 직접 수행했습니다.

실제 협업 환경에서는 협업 가이드를 따르며 버전 관리를 운영할 수 있습니다.
필요 시 구조 설계, 네트워크 흐름 설명, 구조도 등을 PDF로 제공해드릴 수 있습니다.



### 📂 주요 파일 구조
```bash
├── Program.cs               # 서버 진입점
├── NetworkManager.cs        # LiteNetLib 이벤트 처리
├── Matchmaker.cs            # 대기열 관리 및 세션 생성
├── GameSession.cs           # 게임 로직 처리 및 이벤트 발생
├── MessageSender.cs         # 클라이언트 응답 전송
├── PacketParser.cs          # 유틸리티 파서
├── Shared
│   ├── Bits
│   │   ├── BitWriter.cs     # 비트 단위 직렬화
│   │   └── BitReader.cs     # 비트 단위 역직렬화
│   ├── Network
│   │   └── Player.cs        # 유저 상태 및 메타 정보
│   └── Protocol
│       ├── PacketType.cs    # 패킷 구분 Enum
│       └── MapType.cs       # 맵 종류 Enum
```

### 🗂️ 라이선스
MIT License



### 🙋 Contact
이 프로젝트에 대해 궁금한 점이 있다면 아래로 연락주세요:

Email: pgrain@cau.ac.kr

GitHub: 1Kirl
