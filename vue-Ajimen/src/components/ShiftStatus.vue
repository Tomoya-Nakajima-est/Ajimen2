<template>
<div class="shift-status">
    <h2 class="status-header">{{ isWorkingDay ? 'ー今日は出勤日ですー' : 'ー今日はお休みですー' }}</h2>
    <div class="shift-info-box" :class="{ 'working': isWorkingDay, 'off': !isWorkingDay }">
        <p>{{ today }}</p>
        <p v-if="isWorkingDay">シフト{{ shiftType }}：{{ shiftTime }}</p>
    </div>
    
    <div class="week-calendar">
        <span v-for="day in weekDays" :key="day" :class="{ today: day === currentDay }">{{ day }}</span>
    </div>

    <div class="current-time">現在時刻：{{ currentTime }}</div>
    <div class="buttons">
        <button class="clock-in" @click="clockIn">出勤</button>
        <button class="clock-out" @click="clockOut">退勤</button>
    </div>
    </div>
</template>


<script setup>
import{ ref, onMounted } from 'vue'
import axios from 'axios'

//ログインユーザーIDを取得
const userId = localStorage.getItem('userId')

//データ保存
const attendance = ref(null)
const today = ref('')
const shiftType = ref('')
const shiftTime = ref('')
const currentTime = ref('')
const isClockedIn = ref(false)
const isClockedOut = ref(false)

//現在時刻の更新
const updateTime = () => {
    const now = new Date()
    currentTime.value = now.toLocaleTimeString('ja-JP', { hour: '2-digit', minute: '2-digit'})
}

//出勤処理

const clockIn = async () => {
    if (!attendance.value) return
    try {
        await axios.post('/api/attendance/clockin', {
            AttendanceId: attendance.value.attendanceId,
            StaffId: userId
        })
        isClockedIn.value = true
        showNotification('出勤打刻が完了しました！')
    } catch (err) {
        showNotification('出勤打刻に失敗しました')
    }
}


//退勤処理

const clockOut = async () => {
    if (!attendance.value) return
    try {
        await axios.post('/api/attendance/clockout', {
            AttendanceId: attendance.value.attendanceId,
            StaffId: userId
        })
        isClockedOut.value = true
        showNotification('退勤打刻が完了しました！')
    } catch (err) {
        showNotification('退勤打刻に失敗しました')
    }
}


onMounted(async () => {
    updateTime()
    setInterval(updateTime, 1000)

    try {
    const res = await axios.get(`/api/attendance/${userId}`)
    attendance.value = res.data

    // 表示用データ整形
    today.value = new Date(attendance.value.shiftDay).toLocaleDateString('ja-JP', {
        year: 'numeric', month: 'long', day: 'numeric', weekday: 'short'
    })
    shiftType.value = attendance.value.shiftSelect
    shiftTime.value = shiftType.value === 'A' ? '9:00〜13:00' :
    shiftType.value === 'B' ? '16:00〜20:00' : '未定'
    isClockedIn.value = attendance.value.useworkLog !== null
} catch (err) {
    console.error('打刻情報の取得に失敗しました', err)
}
})
</script>

<style scoped>

.shift-status {
    width: 100%;
    max-width: 600px;
    margin: 0 auto;
    padding: 50px;
    background-color: #fff;
    border-radius: 16px;
    text-align: center;
    font-family: 'Georgia', serif;
    display: flex;
    flex-direction: column;
    gap: 30px;
    height: 85vh;
    justify-content: center;
}

.status-header {
    font-size: 32px;
    color: #460125;
}

.shift-info-box {
    padding: 24px;
    border-radius: 10px;
    font-size: 22px;
}

.shift-info-box.working {
    background-color: #f1ebe5;
}

.shift-info-box.off {
    background-color: #eee;
    color: #999;
}

.current-time {
    font-size: 24px;
}

.buttons {
    display: flex;
    justify-content: center;
    gap: 30px;
}

button {
    padding: 18px 36px;
    font-size: 20px;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    transition: background-color 0.3s;
}

.clock-in {
    background-color: #e0f7e9;
}

.clock-out {
    background-color: #fde0e0;
}

.clock-in:hover {
    background-color: #028760;
    color: white;
}

.clock-out:hover {
    background-color: #e9546e;
    color: white;
}

/* スマホ対応 */
@media (max-width: 768px) {
    .shift-status {
        padding: 30px;
        height: auto;
    }
    
    .status-header {
        font-size: 24px;
    }
    
    .shift-info-box {
        font-size: 18px;
    }
    
    .current-time {
        font-size: 20px;
    }
    
    .buttons {
        flex-direction: column;
        gap: 20px;
    }
    button {
        width: 100%;
        font-size: 18px;
    }
}


</style>


