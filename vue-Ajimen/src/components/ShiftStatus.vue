<template>
<div class="shift-status">
    <h2 class="status-header">{{ isWorkingDay ? 'ー今日は出勤日ですー' : 'ー今日はお休みですー' }}</h2>
    <div class="shift-info-box" :class="{ 'working': isWorkingDay, 'off': !isWorkingDay }">
        <p>{{ today }}</p>
        <p v-if="isWorkingDay">シフト{{ shiftType }}：{{ shiftTime }}</p>
    </div>
    <div class="text-center mt-4">
        <button class="btn-history" @click="goToHistory">
            シフト履歴を見る
        </button>

    </div>

    

    <div class="current-time">現在時刻：{{ currentTime }}</div>
    <div class="buttons">
        <button class="clock-in" :class="{ active: isClockedIn }" @click="clockIn">出勤</button>
        <button class="clock-out" :class="{ active: isClockedOut }" @click="clockOut">退勤</button>
    </div>
    </div>
    <NotificationModal
    :visible="notificationVisible"
    :message="notification"
    @close="notificationVisible = false"
/>

</template>


<script setup>
import{ ref, onMounted } from 'vue'
import axios from 'axios'
import NotificationModal from '@/components/NotificationModal.vue'

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
const notificationVisible = ref(false)

const showNotification = (msg) => {
    notification.value = msg
    notificationVisible.value = true
}


//シフト情報の切り替え
const todayISO = new Date().toISOString().split('T')[0]

//履歴画面切り替え
const goToHistory = () => {
    window.location.href = '/ShiftHistory'; // 必要に応じてルーティング調整
}


//状態管理
const isProcessing = ref(false)




//現在時刻の更新
const updateTime = () => {
    const now = new Date()
    currentTime.value = now.toLocaleTimeString('ja-JP', { hour: '2-digit', minute: '2-digit'})
}

//出勤処理

const clockIn = async () => {
    await refreshAttendance()
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
        await refreshAttendance()//状態更新
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
        await refreshAttendance()//状態更新
        showNotification('退勤打刻が完了しました！')
    } catch (err) {
        showNotification('退勤打刻に失敗しました')
    }finally{
        isProcessing.value = false
    }
}


const refreshAttendance = async () => {
    try {
        const res = await axios.get(`http://localhost:5022/api/attendance/list?staffId=${userId}&year=${new Date().getFullYear()}&month=${new Date().getMonth() + 1}`)
        attendanceList.value = res.data.filter(a => a.shiftDay === todayISO)

        // const latest = attendanceList.value
        // .filter(a => a.isWorking)
        // .sort((a, b) => b.startTime.localeCompare(a.startTime))[0]
        const latest = attendanceList.value
        .sort((a, b) => b.startTime.localeCompare(a.startTime))[0]


        // isClockedIn.value = !!latest
        // isClockedOut.value = !latest
        if(latest){
            isClockedIn.value = latest.isWorking
            isClockedOut.value = !latest.isWorking && latest.endTime !== null
        }
        // isClockedIn.value = attendanceList.value.some(a => a.isWorking)
        // isClockedOut.value = attendanceList.value.every(a => !a.isWorking)
        isClockedIn.value = false
        isClockedOut.value = false
    } catch (err) {
        showNotification('勤怠情報の取得に失敗しました')
    }
}



onMounted(async () => {
    updateTime()
    setInterval(updateTime, 1000)

    loadSummary()//一括取得
})



const loadSummary = async () => {
    try {
        const res = await axios.get(`http://localhost:5022/api/attendance/summary?staffId=${userId}`)
        const data = res.data

        isWorkingDay.value = data.isWorkingDay
        shiftType.value = data.shiftType
        shiftTime.value = data.shiftTime
        isClockedIn.value = data.isClockedIn
        isClockedOut.value = data.isClockedOut

        today.value = new Date(data.date).toLocaleDateString('ja-JP', {
            year: 'numeric', month: 'long', day: 'numeric', weekday: 'short'
        })
    } catch (err) {
        console.error('summary API エラー:', err)
        showNotification('現在情報の取得に失敗しました')
    }
}


</script>

<style scoped>

.shift-status {
    width: 100%;
    max-width: 1100px;
    margin: 44px auto 140px auto; 
    padding: 27px 190px;
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


.btn-history {
    background-color: #586068; /* ネイビーグレー系 */
    color: #fff;
    padding: 16px 32px;
    font-size: 20px;
    border: none;
    border-radius: 10px;
    cursor: pointer;
    transition: background-color 0.3s;
    font-family: 'Georgia', serif;
    margin-top: -10px;
}

.btn-history:hover {
    background-color: #262b30;
    color: #f8e58c;
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


