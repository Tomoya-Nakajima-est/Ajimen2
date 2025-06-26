<template>
  <div class="shift-wrapper">
    <div class="calendar-nav">
      <button @click="prevMonth">◀</button>
      <div class="nav-texts">
        <div v-if="mode === 'edit'" class="edit-label">編集したい日付を選択して下さい</div>
        <div>{{ currentYear }}年 {{ currentMonth + 1 }}月のカレンダー</div>
      </div>
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
            v-for="(day, i) in week"
            :key="day.date ? day.date.toISOString() : 'empty-' + w + '-' + i"
            :class="{
              'has-shift': isShiftDay(day.date),
              'selected-date': isSelected(day.date),
              'edited-date': isEdited(day.date)
            }"
            @click="selectDate(day.date)"
          >
            {{ day.label }}
          </td>
        </tr>
      </tbody>
    </table>

    <div class="buttons">
      <button v-if="mode === 'view'" @click="mode = 'select'">申請</button>
      <button v-if="mode === 'view'" @click="startEdit">編集</button>

      <div v-if="mode === 'select'">
        <p class="instruction">～申請したい日付を選択して下さい～</p>
        <button @click="proceedToShiftSelection">申請</button>
        <button @click="reset">取消</button>
      </div>

      <div v-if="mode === 'edit'">
        <p class="instruction">～編集したい日付を選択して下さい～</p>
        <button @click="goToEditShift">編集</button>
        <button @click="reset">取消</button>
      </div>
    </div>

    <!-- 編集モーダル -->
    <div v-if="showEditModal" class="modal">
      <div class="modal-content">
        <button class="close" @click="closeModal">×</button>
        <h3>～シフトを編集してください～</h3>
        <p>{{ formatDate(selectedDate.value) }}</p>

        <div v-if="Array.isArray(shiftAMembers) && shiftAMembers.length > 0">
          <p>・シフト A</p>
          <div v-for="member in allMembers.filter(m => shiftAMembers.includes(m.id))" :key="'A-' + member.id">
            <label>
              <input type="checkbox" :value="member.id" v-model="selectedAMembers"> {{ member.name }}
            </label>
          </div>
        </div>

        <div v-if="Array.isArray(shiftBMembers) && shiftBMembers.length > 0">
          <p>・シフト B</p>
          <div v-for="member in allMembers.filter(m => shiftBMembers.includes(m.id))" :key="'B-' + member.id">
            <label>
              <input type="checkbox" :value="member.id" v-model="selectedBMembers"> {{ member.name }}
            </label>
          </div>
        </div>

        <button class="register" @click="submitEdit">編集</button>
      </div>
    </div>

    <!-- 編集確定確認モーダル -->
    <div v-if="confirmEditModal" class="modal">
      <div class="modal-content confirm">
        <button class="close" @click="confirmEditModal = false">×</button>
        <h3>－シフトの編集を完了しますか？－</h3>

          <div class="edit-list">
          <ul>
            <li v-for="(item, index) in editedConfirmList" :key="index">
              {{ formatDate(item.date) }} / {{ item.name }} / シフト{{ item.shift }}
            </li>
          </ul>
          </div>

        <div class="modal-buttons">
          <button class="register" @click="finalizeEdit">完了</button>
          <button @click="confirmEditModal = false">戻る</button>
        </div>
      </div>
    </div>
    <!-- 編集完了モーダル -->
    <div v-if="showCompleteModal" class="modal">
      <div class="modal-content">
        <button class="close" @click="showCompleteModal = false">×</button>
        <p>－シフトが編集されました－</p>
        <p>－ユーザーに通知されました－</p>
        <button @click="showCompleteModal = false">閉じる</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick } from 'vue'
import axios from 'axios'

const current = new Date()
const currentYear = ref(current.getFullYear())
const currentMonth = ref(current.getMonth())
const calendar = ref([])

const shiftDays = ref({})
const editedDates = ref([])
const selectedDate = ref(null)
const mode = ref('view')

const showEditModal = ref(false)

const shiftAMembers = ref([])
const shiftBMembers = ref([])
const selectedAMembers = ref([])
const selectedBMembers = ref([])

const allMembers = ref([])

const confirmEditModal = ref(false)
const showCompleteModal = ref(false)
const editedConfirmList = ref([])

const getMemberNameById = (id) => {
  const member = allMembers.value.find(m => m.id === id)
  return member ? member.name : id
}

const fetchAllMembers = async () => {
  const res = await axios.get('http://localhost:5022/api/user/all')
  allMembers.value = res.data.map(u => ({ id: u.userName, name: u.userFullName }))
}

const weekDays = ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT']

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
  const res = await axios.get(`http://localhost:5022/api/shift/all`)
  shiftDays.value = {}
  res.data.forEach(item => {
    const dateStr = item.shiftDay.split('T')[0]
    if (!shiftDays.value[dateStr]) shiftDays.value[dateStr] = []
    shiftDays.value[dateStr].push(item.shiftSelect)
  })
}

const toDateString = (date) => {
  if (!(date instanceof Date)) return ''
  const yyyy = date.getFullYear()
  const mm = String(date.getMonth() + 1).padStart(2, '0')
  const dd = String(date.getDate()).padStart(2, '0')
  return `${yyyy}-${mm}-${dd}`
}

const isShiftDay = (date) => {
  const shifts = shiftDays.value[toDateString(date)]
  return Array.isArray(shifts) && shifts.length > 0
}
const isSelected = (date) => date && selectedDate.value && toDateString(date) === toDateString(selectedDate.value)
const isEdited = (date) => {
  const d = toDateString(date)
  return editedDates.value.includes(d)
}

const selectDate = async (date) => {
  if (!date) return
  selectedDate.value = date

  if (mode.value === 'edit' && isShiftDay(date)) {
    await fetchShiftMembers(toDateString(date))
    showEditModal.value = true
  }
}

const fetchShiftMembers = async (dateStr) => {
  const [resA, resB] = await Promise.all([
    axios.get(`http://localhost:5022/api/shift/editview`, { params: { day: dateStr, shiftSelect: 'A' } }),
    axios.get(`http://localhost:5022/api/shift/editview`, { params: { day: dateStr, shiftSelect: 'B' } })
  ])
  shiftAMembers.value = resA.data ?? []
  shiftBMembers.value = resB.data ?? []
  selectedAMembers.value = [...shiftAMembers.value]
  selectedBMembers.value = [...shiftBMembers.value]
}

const submitEdit = async () => {
  const hasSelection = selectedAMembers.value.length > 0 || selectedBMembers.value.length > 0
  if (!hasSelection) {
    alert('編集する人を選んでください')
    return
  }

  // 編集対象リストを準備
  editedConfirmList.value = []
  const date = selectedDate.value

  selectedAMembers.value.forEach(id => {
    editedConfirmList.value.push({
      date,
      shift: 'A',
      name: getMemberNameById(id),
    })
  })
  selectedBMembers.value.forEach(id => {
    editedConfirmList.value.push({
      date,
      shift: 'B',
      name: getMemberNameById(id),
    })
  })

  showEditModal.value = false
  confirmEditModal.value = true
}

const finalizeEdit = async () => {
  // 日付を編集済みとして記録
  const dateStr = toDateString(selectedDate.value)
  if (!editedDates.value.includes(dateStr)) {
    editedDates.value.push(dateStr)
  }

  confirmEditModal.value = false
  showCompleteModal.value = true
  selectedDate.value = null
}

const startEdit = () => mode.value = 'edit'
const reset = () => {
  mode.value = 'view'
  selectedDate.value = null
  showEditModal.value = false
  editedDates.value = []  // ← ここで「編集済み色」をリセット！
}
const proceedToShiftSelection = () => alert('Shift Selectへ遷移')
const goToEditShift = () => {
  if (!selectedDate.value) return alert('日付を選択してください')
  showEditModal.value = true
}

const formatDate = (date) => {
  if (!date) return ''
  const days = ['日', '月', '火', '水', '木', '金', '土']
  return `${date.getMonth() + 1}月${date.getDate()}日（${days[date.getDay()]}）`
}

const changeMonth = (offset) => {
  const newDate = new Date(currentYear.value, currentMonth.value + offset, 1)
  currentYear.value = newDate.getFullYear()
  currentMonth.value = newDate.getMonth()
}
const prevMonth = () => changeMonth(-1)
const nextMonth = () => changeMonth(1)

const closeModal = () => showEditModal.value = false

onMounted(async () => {
  await fetchAllMembers()
  buildCalendar()
  await loadShifts()
})

watch([currentYear, currentMonth], async () => {
  buildCalendar()
  await loadShifts()
})
</script>

<style scoped>
.shift-wrapper {
  background-color: #fefaf6;
  padding: 2.5rem 3rem;
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
.selected-date {
  background-color: #7fe5ff;
}
.edited-date {
  background-color: #c922b8 !important;
  color: white !important;
  font-weight: bold;
  border: 2px solid #880f6c !important;
}
.has-shift {
  background-color: #ffe08c;
}
.buttons {
  text-align: center;
  margin-top: 1rem;
}
.instruction {
  font-weight: bold;
  margin: 1rem 0;
  text-align: center;
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
.register {
  margin-top: 1rem;
  background-color: #28a745;
  color: white;
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
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
.nav-texts {
  display: flex;
  flex-direction: column;
  align-items: center;
}
.edit-label {
  font-size: 0.9rem;
  color: #e67e22;
  font-weight: bold;
}

.modal-content.confirm {
  max-height: 80vh;
  overflow-y: auto;
}

.edit-list {
  background: white;
  margin: 1rem 0;
  padding: 1rem;
  border-radius: 6px;
  max-height: 200px;
  overflow-y: auto;
  text-align: left;
}

.edit-list ul {
  list-style-type: none;
  padding: 0;
  margin: 0;
}

.edit-list li {
  padding: 0.5rem 0;
  border-bottom: 1px solid #ddd;
}

.modal-buttons {
  display: flex;
  justify-content: center;
  gap: 1.5rem;
  margin-top: 1rem;
}
</style>