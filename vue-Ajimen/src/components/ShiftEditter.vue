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
                'edited-date': isEdited(day.date),
                'confirmed-date': isConfirmedDay(day.date)
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
                <button @click="removeEditRecord(index)" style="margin-left: 1rem; color: red;">×</button>
        </li>
      </ul>
    </div>

    <div class="modal-buttons">
      <button class="register" @click="finalizeEdit">完了</button>
      <button @click="confirmEditModal = false">戻る</button>
      <button @click="cancelAllEdits" style="color: red;">すべて取り消し</button>
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
    <!-- 申請シフト選択モーダル -->
    <div v-if="mode === 'choose-shift'" class="modal">
      <div class="modal-content">
        <button class="close" @click="mode = 'select'">×</button>
        <h3>～シフトを選択してください～</h3>
        <p>{{ formatDate(selectedDate.value) }}</p>
        <button :class="{ 'selected-shift': shiftSelect === 'A' }" @click="shiftSelect = 'A'">シフトA</button>
        <button :class="{ 'selected-shift': shiftSelect === 'B' }" @click="shiftSelect = 'B'">シフトB</button>
        <button class="register" @click="mode = 'confirm-apply'">登録</button>
      </div>
    </div>

    <!-- 確認モーダル -->
    <div v-if="mode === 'confirm-apply'" class="modal">
      <div class="modal-content">
        <button class="close" @click="mode = 'choose-shift'">×</button>
        <h3>～このシフトで申請しますか？～</h3>
        <p>{{ formatDate(selectedDate.value) }}：シフト{{ shiftSelect }}</p>
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
import { ref, onMounted, watch, nextTick } from 'vue'
import axios from 'axios'
import ShiftTable from '@/components/ShiftTable.vue'

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

const selectedDates = ref([]) // 複数日付を保持
const editedRecords = ref([]) // 編集結果を蓄積

const shiftSelect = ref('')
const userId = localStorage.getItem('userId')


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

const isConfirmedDay = (date) => {
  const dateStr = toDateString(date);
  const shifts = shiftDays.value[dateStr];
  return Array.isArray(shifts) && shifts.some(s => s.isConfirmed);
};

const loadShifts = async () => {
  const res = await axios.get(`http://localhost:5022/api/shift/all`);
  shiftDays.value = {};

  res.data.forEach(item => {
    const dateStr = item.shiftDay.split('T')[0];
    if (!shiftDays.value[dateStr]) shiftDays.value[dateStr] = [];

    shiftDays.value[dateStr].push({
      shiftId: item.shiftId,
      memberId: item.memberId,
      shiftSelect: item.shiftSelect,
      isConfirmed: item.isConfirmed
    });
  });
};

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

const isSelected = (date) => {
  if (!date) return false
  return selectedDates.value.some(d => toDateString(d) === toDateString(date))
}

const isEdited = (date) => {
  const d = toDateString(date)
  return editedDates.value.includes(d)
}

const selectDate = async (date) => {
  if (!date) return;

  // ★ 追加：確定済み日付はクリック不可
  if (isConfirmedDay(date)) {
    alert("このシフトはすでに確定済みです。編集できません。");
    return;
  }

  // 申請モード時の処理（モーダル遷移）
  if (mode.value === 'select') {
    selectedDate.value = date;
    mode.value = 'choose-shift';
    return;
  }

  // 編集モード時の処理
  if (mode.value === 'edit' && isShiftDay(date)) {
    selectedDate.value = date;
    await fetchShiftMembers(toDateString(date));
    showEditModal.value = true;
  }
};

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
  const date = selectedDate.value
  if (!date || (selectedAMembers.value.length === 0 && selectedBMembers.value.length === 0)) {
    alert('編集対象を選んでください')
    return
  }

  // 重複削除 & 編集記録保存
  if (!selectedDates.value.some(d => toDateString(d) === toDateString(date))) {
    selectedDates.value.push(date)
  }
  if (!editedDates.value.includes(toDateString(date))) {
    editedDates.value.push(toDateString(date))
  }

  // 編集リストへ記録
  selectedAMembers.value.forEach(id => {
    const shiftId = getShiftIdByDateAndType(selectedDate.value, 'A')
    editedRecords.value.push({ date, name: getMemberNameById(id), shift: 'A', shiftId })
  })
  selectedBMembers.value.forEach(id => {
    const shiftId = getShiftIdByDateAndType(selectedDate.value, 'B')
    editedRecords.value.push({ date, name: getMemberNameById(id), shift: 'B', shiftId })
  })

  showEditModal.value = false
  selectedDate.value = null
}

const getShiftIdByDateAndType = (date, shiftType) => {
  const dateStr = toDateString(date)
  const shifts = shiftDays.value[dateStr]
  if (!Array.isArray(shifts)) return null
  const match = shifts.find(s => s.shiftSelect === shiftType)
  return match ? match.shiftId : null
}

const finalizeEdit = async () => {
  confirmEditModal.value = false

  // ✅ shiftId だけを取り出す
  const shiftIdList = [...new Set(editedRecords.value.map(r => r.shiftId).filter(id => id))]

  try {
    if (shiftIdList.length > 0) {
      await axios.post('http://localhost:5022/api/shift/confirm-by-id', shiftIdList)
    }
    showCompleteModal.value = true
    await loadShifts()
  } catch (error) {
    alert("確定処理に失敗しました: " + error.message)
  }

  selectedDates.value = []
  editedRecords.value = []
}

const startEdit = () => mode.value = 'edit'

const reset = () => {
  selectedDate.value = null;
  shiftSelect.value = '';
  showEditModal.value = false;
  confirmEditModal.value = false;
  showCompleteModal.value = false;
  editedConfirmList.value = [];
  selectedDates.value = [];
  editedDates.value = [];
  editedRecords.value = [];

  // ★★ ここを明示的に追加 ★★
  mode.value = 'view';
};

const proceedToShiftSelection = () => alert('Shift Selectへ遷移')

const goToEditShift = () => {
  if (editedRecords.value.length === 0) {
    alert('編集内容がありません')
    return
  }
  confirmEditModal.value = true
  editedConfirmList.value = [...editedRecords.value]
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

const closeModal = () => {
  showEditModal.value = false
  selectedDate.value = null
}

const removeEditRecord = (indexToRemove) => {
  const record = editedRecords.value[indexToRemove]
  editedRecords.value.splice(indexToRemove, 1)

  const dateStr = toDateString(record.date)

  // 編集がその日付にもう無ければ、該当日付を削除
  const remainingForDate = editedRecords.value.filter(r => toDateString(r.date) === dateStr)
  if (remainingForDate.length === 0) {
    editedDates.value = editedDates.value.filter(d => d !== dateStr)
    selectedDates.value = selectedDates.value.filter(d => toDateString(d) !== dateStr)
  }

  // 更新表示用
  editedConfirmList.value = [...editedRecords.value]
}

const cancelAllEdits = () => {
  editedRecords.value = []
  editedDates.value = []
  selectedDates.value = []
  editedConfirmList.value = []
  confirmEditModal.value = false  // モーダルを閉じる
}

const goToSelectShift = () => {
  if (!selectedDate.value) return alert('日付を選択してください')
  mode.value = 'choose-shift'
}

const applyShift = async () => {
  const date = new Date(selectedDate.value);
  date.setHours(date.getHours() - date.getTimezoneOffset() / 60);
  const yyyy = date.getFullYear();
  const mm = String(date.getMonth() + 1).padStart(2, '0');
  const dd = String(date.getDate()).padStart(2, '0');
  const shiftDayString = `${yyyy}-${mm}-${dd}T00:00:00`;

  await axios.post('http://localhost:5022/api/shift/apply', {
    day: shiftDayString,
    shiftSelect: shiftSelect.value,
    memberId: userId,
  });

  await loadShifts();

  // ★ 申請完了 → 完了モーダル → 閉じたときに view に戻る
  mode.value = 'done';
};

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

.modal-content button.selected-shift {
  background-color: #5ab2ff;
  color: white;
}

.confirmed-date {
  background-color: #c922b8 !important;
  color: white !important;
  font-weight: bold;
  border: 2px solid #880f6c !important;
}
</style>