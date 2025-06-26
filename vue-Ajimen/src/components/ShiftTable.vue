<template>
  <div class="shift-wrapper">
    <div class="calendar-nav">
      <button @click="prevMonth">◀</button>
      <span>{{ currentYear }}年 {{ currentMonth + 1 }}月のカレンダー</span>
      <button @click="nextMonth">▶</button>
    </div>
    <table class="calendar">
      <thead>
        <tr>
          <th v-for="(d, i) in weekDays" :key="i">{{ d }}</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(week, w) in calendar" :key="w">
          <td
          v-for="day in week"
          :key="day.date"
          :class="{
            'has-shift': day.date && isInCurrentMonth(day.date) && shiftDays.includes(toDateString(day.date)),
            'selected-date': mode === 'select' && day.date && toDateString(day.date) === toDateString(selectedDate)
          }"
  @click="selectDate(day.date)"
>
  {{ day.label }}
</td>
        </tr>
      </tbody>
    </table>

    <div v-if="mode === 'view'" class="buttons">
      <button @click="mode = 'select'">シフトを申請する</button>
    </div>

    <div v-if="mode === 'select'" class="buttons">
      <p class="instruction">～申請したい日付を選択して下さい～</p>
      <button @click="goToSelectShift">申請</button>
      <button @click="cancel">取消</button>
    </div>

    <!-- シフト選択モーダル -->
    <div v-if="mode === 'choose-shift'" class="modal">
      <div class="modal-content">
        <button class="close" @click="mode = 'select'">&times;</button>
        <h3>～シフトを選択してください～</h3>
        <p>{{ formatDate(selectedDate) }}</p>
        <button :class="{ 'selected-shift': shiftSelect === 'A' }" @click="shiftSelect = 'A'">シフトA</button>
        <button :class="{ 'selected-shift': shiftSelect === 'B' }" @click="shiftSelect = 'B'">シフトB</button>
        <button class="register" @click="mode = 'confirm'">登録</button>
      </div>
    </div>

    <!-- 確認モーダル -->
    <div v-if="mode === 'confirm'" class="modal">
      <div class="modal-content">
        <button class="close" @click="mode = 'choose-shift'">×</button>
        <h3>～このシフトで申請しますか？～</h3>
        <p>{{ formatDate(selectedDate) }}：シフト{{ shiftSelect }}</p>
        <button class="register" @click="applyShift">申請</button>
      </div>
    </div>

    <!-- 完了モーダル -->
    <div v-if="mode === 'done'" class="modal">
      <div class="modal-content">
        <button class="close" @click="reset">×</button>
        <h3>～申請完了しました！～</h3>
        <button @click="reset">閉じる</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'

const current = new Date()
const currentYear = ref(current.getFullYear())
const currentMonth = ref(current.getMonth())
const calendar = ref([])
const weekDays = ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT']
const shiftDays = ref([])
const selectedDate = ref(null)
const shiftSelect = ref('')
const mode = ref('view')
const userId = localStorage.getItem('userId')

const toDateString = (date) => {
  if (!date) return '';
  const yyyy = date.getFullYear();
  const mm = String(date.getMonth() + 1).padStart(2, '0');
  const dd = String(date.getDate()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd}`;
}
const isToday = (date) => {
  const today = new Date()
  return date && toDateString(date) === toDateString(today)
}

const isInCurrentMonth = (date) => {
  return date &&
    date.getFullYear() === currentYear.value &&
    date.getMonth() === currentMonth.value;
}

const buildCalendar = () => {
  const start = new Date(currentYear.value, currentMonth.value, 1)
  const end = new Date(currentYear.value, currentMonth.value + 1, 0)
  const days = []
  let week = []
  for (let i = 0; i < start.getDay(); i++) week.push({ label: '', date: null })
  for (let d = 1; d <= end.getDate(); d++) {
    const date = new Date(currentYear.value, currentMonth.value, d)
    week.push({ label: d, date })
    if (week.length === 7) {
      days.push(week)
      week = []
    }
  }
  if (week.length) days.push(week)
  calendar.value = days
}

const loadShifts = async () => {
  const res = await axios.get(`http://localhost:5022/api/shift/get`, {
    params: {
      year: currentYear.value,
      month: currentMonth.value + 1,
      memberId: userId
    }
  });
  console.log("サーバーからのデータ:", res.data);
  shiftDays.value = res.data.map(item => item.date);
  console.log("shiftDays に格納された内容:", shiftDays.value);
};

const prevMonth = () => {
  if (currentMonth.value === 0) {
    currentYear.value--
    currentMonth.value = 11
  } else {
    currentMonth.value--
  }
  buildCalendar()
  loadShifts()
}
const nextMonth = () => {
  if (currentMonth.value === 11) {
    currentYear.value++
    currentMonth.value = 0
  } else {
    currentMonth.value++
  }
  buildCalendar()
  loadShifts()
}
const selectDate = (date) => {
  if (!date) return; // ← null 日付対策
  if (mode.value !== 'select') return;
  selectedDate.value = date;
}
const goToSelectShift = () => {
  if (!selectedDate.value) return alert('日付を選択してください')
  mode.value = 'choose-shift'
}
const cancel = () => {
  selectedDate.value = null
  shiftSelect.value = ''
  mode.value = 'view'
}
const applyShift = async () => {
  // JST時間にする（UTCに+9時間）
  const date = new Date(selectedDate.value)
  date.setHours(date.getHours() - date.getTimezoneOffset() / 60) // JST対応

  const yyyy = date.getFullYear()
  const mm = String(date.getMonth() + 1).padStart(2, '0')
  const dd = String(date.getDate()).padStart(2, '0')
  const shiftDayString = `${yyyy}-${mm}-${dd}T00:00:00`

  await axios.post('http://localhost:5022/api/shift/apply', {
    day: shiftDayString,
    shiftSelect: shiftSelect.value,
    memberId: userId
  })

  await loadShifts()
  mode.value = 'done'
}

const reset = () => {
  selectedDate.value = null
  shiftSelect.value = ''
  mode.value = 'view'
}
const formatDate = (date) => {
  const weekday = ['日', '月', '火', '水', '木', '金', '土']
  return `${date.getMonth() + 1}月${date.getDate()}日（${weekday[date.getDay()]}）`
}
onMounted(() => {
  buildCalendar()
  loadShifts()
})
</script>

<style scoped>
.shift-wrapper {
  background-color: #fefaf6; /* クリーム色 */
  padding: 2.5rem 3rem;
  margin-top: 0; /* ← ここを調整 */
  border-radius: 8px;
  width: fit-content;
  min-width: 600px;
  min-height: 500px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-start;
  box-shadow: 0 0 12px rgba(0, 0, 0, 0.1);
}

.calendar-nav {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
  margin: 1rem 0;
  font-size: 1.1rem;
  font-weight: bold;
}

.calendar-nav span {
  white-space: nowrap;
}

table.calendar {
  border-collapse: collapse;
  width: 100%;
  background: white;
  border-radius: 6px;
  overflow: hidden;
  margin-bottom: 1rem;
}

th, td {
  border: 1px solid #ccc;
  width: 14.2%;
  height: 3rem;
  text-align: center;
  cursor: pointer;
}

.calendar td.selected-date {
  background-color: #7fe5ff;
}

.calendar td.has-shift {
  background-color: #ffe08c;
}

.modal {
  position: fixed;
  top: 0; left: 0;
  width: 100%; height: 100%;
  background: rgba(0,0,0,0.3);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 999;
}

.modal-content {
  background: #fefaf6;
  padding: 2rem;
  border-radius: 8px;
  text-align: center;
  position: relative;
}

.modal-content button {
  margin: 0.5rem;
}

.modal-content button.selected-shift {
  background-color: #5ab2ff;
  color: white;
}

.close {
  position: absolute;
  right: 1rem;
  top: 0.5rem;
  background: none;
  border: none;
  font-size: 1.2rem;
  cursor: pointer;
}

.register {
  margin-top: 1rem;
  background-color: #28a745;
  color: white;
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
}

.instruction {
  text-align: center;
  margin-bottom: 1rem;
  font-weight: bold;
}

.buttons {
  text-align: center;
  margin-top: 1rem;
}
</style>