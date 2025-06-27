<template>
  <div class="main-area">
    <!-- 在庫表 -->
    <section>
      <h2 class="section-title">－在庫表－</h2>
      <div class="stock-table-wrap">
        <div class="search-row">
          <input class="search-input" placeholder="検索の際はここに入力" v-model="searchKeyword"/>
          <button class="search-btn" @click="searchStock">検索</button>
          <button class="search-btn" @click="searchKeyword=''; searchStock();">クリア</button>
        </div>
        <div class="table-scroll">
          <table class="table" id="stockTable">
            <thead>
              <tr>
                <th>在庫ログID</th>
                <th>登録者ID</th>
                <th>登録者</th>
                <th>登録日時</th>
                <th>在庫数</th>
                <th>最低数</th>
                <th>物品ID</th>
                <th>物品名</th>
                <th>種別</th>
                <th>　</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="stock in filteredStockList" :key="stock.stockLogId"
                  :class="{'low-stock':stock.stockNum < stock.stockMinNum}">
                <td>{{ stock.stockLogId }}</td>
                <td>{{ stock.staffId }}</td>
                <td>{{ stock.staffName }}</td>
                <td>{{ new Date(stock.stockDay).toLocaleString() }}</td>
                <td>
                  <input
                    type="number"
                    min="0"
                    v-model.number="stock.stockNum"
                    class="input-mini"
                  />
                </td>
                <td>{{ stock.stockMinNum }}</td>
                <td>{{ stock.itemId }}</td>
                <td>{{ stock.itemName }}</td>
                <td>{{ stock.itemKinds }}</td>
                <td>
                  <button class="pill-btn primary" @click="addToOrderList(stock)">発注</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </section>
    <!-- 発注予定 -->
    <section class="order-area">
      <h2 class="section-title">－発注予定－</h2>
      <div class="order-table-wrap">
        <div class="table-scroll">
          <table class="table" id="orderListTable">
            <thead>
              <tr>
                <th>発注ログID</th>
                <th>登録者ID</th>
                <th>登録者</th>
                <th>物品ID</th>
                <th>物品名</th>
                <th>種別</th>
                <th>発注数</th>
                <th>削除</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(order, idx) in orderList" :key="order.ItemId">
                <td>{{ idx + 1 }}</td>
                <td>{{ order.StaffId }}</td>
                <td>{{ order.StaffName }}</td>
                <td>{{ order.ItemId }}</td>
                <td>{{ order.ItemName }}</td>
                <td>{{ order.ItemKinds }}</td>
                <td>{{ order.OrderNum }}</td>
                <td>
                  <button class="pill-btn danger" @click="removeOrder(order.ItemId)">削除</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <div class="order-btn-row">
        <button class="order-btn-big" @click="submitOrders" :disabled="orderList.length === 0">発注</button>
        <button class="cancel-btn-big" @click="resetOrderList">取消</button>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'

const stockList = ref([])
const filteredStockList = ref([])
const searchKeyword = ref('')
const orderList = ref([])

onMounted(async () => {
  const res = await axios.get('http://localhost:5022/api/stock/logs')
  stockList.value = res.data
  filteredStockList.value = res.data
})

function searchStock() {
  const keyword = searchKeyword.value.trim().toLowerCase()
  if (!keyword) {
    filteredStockList.value = [...stockList.value]
    return
  }
  filteredStockList.value = stockList.value.filter(stock =>
    String(stock.itemId).includes(keyword) ||
    String(stock.itemName).toLowerCase().includes(keyword)
  )
}

function addToOrderList(item) {
  const orderNum = prompt("発注数を入力してください", 1)
  if (orderNum !== null && !isNaN(Number(orderNum))) {
    const staffId = localStorage.getItem('userId')
    const staffName = localStorage.getItem('userFullName')
    const existing = orderList.value.find(o => o.ItemId === item.itemId)
    const orderData = {
      OrderLogId: 0,
      StaffId: staffId,
      StaffName: staffName,
      OrderNum: Number(orderNum),
      ItemId: item.itemId,
      ItemName: item.itemName,
      ItemKinds: item.itemKinds || "未分類",
    }
    if (existing) {
      Object.assign(existing, orderData)
    } else {
      orderList.value.push(orderData)
    }
  }
}

function removeOrder(itemId) {
  orderList.value = orderList.value.filter(o => o.ItemId !== itemId)
}
function resetOrderList() {
  orderList.value = []
}
async function submitOrders() {
  if (orderList.value.length === 0) return
  try {
    const res = await axios.post('http://localhost:5022/api/orderlog/submitandmail', orderList.value)
    if (res.status === 200) {
      alert("発注内容を保存し、メール送信に成功しました。\n" + (res.data || ""));
      orderList.value = []
    } else {
      alert("サーバーがエラー応答を返しました: " + (res.data || "詳細不明"))
    }
  } catch (e) {
    let msg = "保存またはメール送信時にエラーが発生しました。"
    if (e.response && e.response.data) {
      msg += "\n詳細: " + e.response.data
    }
    alert(msg)
  }
}
</script>

<style scoped>
.main-area {
  position: relative;
  padding-left: 240px;     /* サイドバー分を常に空ける */
  padding-right: 120px;
  min-height: 100vh;
  width: 100vw;
  box-sizing: border-box;
  overflow-x: auto;
  /* 背景色は付けない！ */
}

/* 表全体を白背景で浮かせる */
.stock-table-wrap,
.order-table-wrap {
  background: #fff;
  border-radius: 14px;
  box-shadow: 0 2px 12px #bbb8b833;
  margin-bottom: 24px;
  padding: 18px 1vw 12px 1vw;
  position: relative;
  z-index: 2;
}

/* テーブルのスクロール部分 */
.table-scroll {
  width: 100%;
  overflow-x: auto;
}

/* テーブル共通 */
.table {
  width: 100%;
  min-width: 640px;
  border-collapse: collapse;
  background: #fff;
  font-size: 0.80rem;
}
.table thead tr {
  background: #3a2e4e;
  color: #fff;
}
.table th, .table td {
  border-left: 1px solid #b8a8ad;
  text-align: center;
  padding: 4px 7px;
  white-space: nowrap;
}
.table th:first-child, .table td:first-child { border-left: none; }
.low-stock > td { background-color: #be6f4a !important; color: #fff; }

.input-mini {
  width: 48px;
  font-size: 0.97em;
  padding: 1px 3px;
  box-sizing: border-box;
  text-align: right;
}

.pill-btn {
  border-radius: 999px !important;
  padding: 2px 18px !important;
  border: 1.5px solid #b7d1e5;
  min-width: 40px;
  font-size: 1em;
  cursor: pointer;
  transition: background 0.17s;
}
.pill-btn.primary {
  background: #eaf8ff;
  color: #485278;
}
.pill-btn.primary:hover { background: #d2eefd; }
.pill-btn.danger {
  background: #edd8ed;
  color: #94618e;
  border: 1.5px solid #cbb4ce;
}
.pill-btn.danger:hover { background: #fbe5fa; }

.order-btn-row {
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: center;
  gap: 16px;
  margin: 12px 0 8px 0;
}

.order-btn-big, .cancel-btn-big {
  border-radius: 999px;
  font-size: 1.04rem;
  padding: 8px 32px;
  border: 1.5px solid #b7d1e5;
  margin: 2px 5px;
  cursor: pointer;
}
.order-btn-big { background: #eaf8ff; color: #486578; }
.order-btn-big:hover { background: #d2eefd; }
.cancel-btn-big { background: #edd8ed; color: #94618e; border: 1.5px solid #cbb4ce; }
.cancel-btn-big:hover { background: #fbe5fa; }

.section-title {
  font-family: 'Yu Mincho', serif;
  font-size: 1.3rem;
  text-align: center;
  color: #c0c0c0;
  margin: 8px 0 14px 0;
  letter-spacing: 0.1em;
}

/* スマホなどでサイドバーが隠れるときはmain-area左paddingを0に */
@media (max-width: 700px) {
  .main-area {
    padding-left: 0 !important;
    width: 100vw;
  }
  .table { font-size: 0.68rem; min-width: 420px; }
  .order-btn-big, .cancel-btn-big { font-size: 0.92rem; padding: 7px 18px; }
  .pill-btn { padding: 2px 10px !important; font-size: 0.90em;}
}
</style>
