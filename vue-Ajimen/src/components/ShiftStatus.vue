<template>
<div class="shift-status">
    <h2 class="status-header">{{ isWorkingDay ? 'ー今日は出勤日ですー' : 'ー今日はお休みですー' }}</h2>
    <div class="shift-info-box" :class="{ 'working': isWorkingDay, 'off': !isWorkingDay }">
        <p>{{ today }}</p>
        <p v-if="isWorkingDay">シフト{{ shiftType }}：{{ shiftTime }}</p>
    </div>

    <div v-if="notification" class="popup">{{ notification }}</div>

    <div class="current-time">現在時刻：{{ currentTime }}</div>
    <div class="buttons">
        <button class="clock-in" :class="{ active: isClockedIn }" @click="clockIn">出勤</button>
        <button class="clock-out" :class="{ active: isClockedOut }" @click="clockOut">退勤</button>
    </div>
    </div>
</template>


<script setup>
import{ ref, onMounted } from 'vue'
import axios from 'axios'

//ログインユーザーIDを取得
const userId = localStorage.getItem('userId')
console.log(userId);
//データ保存
const attendanceList = ref([])
const today = ref('')
const shiftType = ref('')
const shiftTime = ref('')
const currentTime = ref('')
const isClockedIn = ref(false)
const isClockedOut = ref(false)
const isWorkingDay = ref(false) 


//ポップアップ通知
const notification = ref('')

const showNotification = (message) => {
    notification.value = message
    setTimeout(() => {
        notification.value = ''
    }, 3000)
}

//シフト情報の切り替え
const todayISO = new Date().toISOString().split('T')[0]

//状態管理
const isProcessing = ref(false)




//現在時刻の更新
const updateTime = () => {
    const now = new Date()
    currentTime.value = now.toLocaleTimeString('ja-JP', { hour: '2-digit', minute: '2-digit'})
}

//出勤処理

const clockIn = async () => {
    if (isProcessing.value || isClockedIn.value){
        showNotification('すでに出勤済み もしくは 処理中です')
        return
    }

    isProcessing.value = true
    try {
        await axios.post('http://localhost:5022/api/Attendance/clock-in',{ staffId: userId },   {
            headers: {
                'Content-Type': 'application/json'
            }
})

        isClockedIn.value = true
        showNotification('出勤打刻が完了しました！')
    } catch (err) {
        showNotification('出勤打刻に失敗しました')
    }finally{
        isProcessing.value = false
    }

    
}


//退勤処理

const clockOut = async () => {
    if (isProcessing.value || isClockedOut.value) {
        showNotification('すでに退勤済み もしくは 処理中です')
        return
    }
    try {
        await axios.post('http://localhost:5022/api/Attendance/clock-out', { staffId: userId },   {
            headers: {
                'Content-Type': 'application/json'
            }
})

        isClockedOut.value = true
        showNotification('退勤打刻が完了しました！')
    } catch (err) {
        showNotification('退勤打刻に失敗しました')
    }finally{
        isProcessing.value
    }
}


const refreshAttendance = async () => {
    try {
        const res = await axios.get(`http://localhost:5022/api/attendance/list?staffId=${userId}&year=${new Date().getFullYear()}&month=${new Date().getMonth() + 1}`)
        attendanceList.value = res.data.filter(a => a.shiftDay === todayISO)

        const latest = attendanceList.value
        .filter(a => a.isWorking)
        .sort((a, b) => b.startTime.localeCompare(a.startTime))[0]

        isClockedIn.value = !!latest
        isClockedOut.value = !latest
    } catch (err) {
        showNotification('勤怠情報の取得に失敗しました')
    }
}



onMounted(async () => {
    updateTime()
    setInterval(updateTime, 1000)
    
    const todayISO = new Date().toISOString().split('T')[0]
    
    try {
        // 勤怠情報の取得
        const res = await axios.get(`http://localhost:5022/api/attendance/today?staffId=${userId}`)
        attendance.value = res.data
        // 日付表示
        today.value = new Date(attendance.value.shiftDay).toLocaleDateString('ja-JP', {
            year: 'numeric', month: 'long', day: 'numeric', weekday: 'short'
        })
        // 出勤状態
        isClockedIn.value = attendance.value.isWorking
        
        // シフト情報の取得
        const shiftRes = await axios.get(`http://localhost:5022/api/shift/check?day=${todayISO}&memberId=${userId}`)
        if (shiftRes.data.length > 0) {
            isWorkingDay.value = true
            shiftType.value = shiftRes.data[0].shiftSelect
            shiftTime.value = shiftType.value === 'A' ? '9:00〜13:00' :
            shiftType.value === 'B' ? '16:00〜20:00' : '未定'
        } else {
            isWorkingDay.value = false
        }
    } catch (err) {
        if (err.response?.status === 404) {
            showNotification('本日の勤怠情報はまだ登録されていません')
        } else {
            showNotification('勤怠情報の取得に失敗しました')
        }
    }
})

</script>

<style scoped>

.shift-status {
    width: 100%;
    max-width: 1100px;
    margin: 50px auto 140px auto; 
    padding: 66px 190px;
    background-color: #fefaf6;
    border-radius: 16px;
    text-align: center;
    font-family: 'Georgia', serif;
    display: flex;
    flex-direction: column;
    gap: 40px;
    height: auto;
    justify-content: center;
}

.status-header {
    font-size: 40px;
    color: #460125;
}

.shift-info-box {
    padding: 20px;
    border-radius: 12px;
    font-size: 20px;
}

.shift-info-box.working {
    background-color: #f1ebe5;
}

.shift-info-box.off {
    background-color: #eee;
    color: #999;
}

.current-time {
    font-size: 28px;
}

.buttons {
    display: flex;
    justify-content: center;
    gap: 30px;
}

button {
    padding: 18px 36px;
    font-size: 22px;
    min-width: 160px;
    border: none;
    border-radius: 10px;
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


.popup {
    position: fixed;
    top: 20px;
    right: 20px;
    background-color: #028760;
    color: white;
    padding: 12px 24px;
    border-radius: 8px;
    font-size: 16px;
    box-shadow: 0 4px 8px rgba(0,0,0,0.2);
        z-index: 1000;
}





/* スマホ対応 */
@media (max-width: 768px) {
    .shift-status {
        padding: 40px 20px; /* 横の余白を小さく */
        height: auto;
    }
    
    .status-header {
        font-size: 22px;
    }
    
    .shift-info-box {
        font-size: 16px;
    }
    
    .current-time {
        font-size: 18px;
    }
    
    .buttons {
        flex-direction: column;
        gap: 16px;
    }
    button {
        width: 100%;
        font-size: 16px;
    }
}




</style>


