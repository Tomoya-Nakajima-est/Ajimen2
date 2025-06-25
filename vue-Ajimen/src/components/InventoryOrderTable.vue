<template>
  <div>
    <!-- 在庫表 -->
    <section>
      <h2 class="section-title">－在庫表－</h2>
      <!-- ここに在庫表テーブル -->
       <table class="stock-table-wrap">
        <thead>
          <tr>
            <th>在庫ログID</th>
            <th>登録者ID</th>
            <th>登録者名</th>
            <th>記録日</th>
            <th>在庫数</th>
            <th>最低在庫数</th>
            <th>物品ID</th>
            <th>物品名</th>
            <th>カテゴリ</th>
            <th>　</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="stock in stockList" :key="stock.stockLogId">
            <td>{{ stock.stockLogId }}</td>
            <td>{{ stock.staffId }}</td>
            <td>{{ stock.staffName }}</td>
            <td>{{ new Date(stock.stockDay).toLocaleString() }}</td>
            <td>{{ stock.stockNum }}</td>
            <td>{{ stock.stockMinNum }}</td>
            <td>{{ stock.itemId }}</td>
            <td>{{ stock.itemName }}</td>
            <td>{{ stock.itemKinds }}</td>
            <td>
              <button @click="addToOrderList(stock)">発注</button>
            </td>
          </tr>
        </tbody>
        <div class="search-row">
          <input class="search-input" placeholder="検索の際はここに入力"/>
          <button class="search-btn">検索</button>
        </div>
      </table>
    </section>
    <!-- 発注予定 -->
       <section class="order-area">
        <h2 class="section-title">－発注予定－</h2>
        <div class="order-table-wrap">
          <table class="table" id="orderListTable">
            <thead>
              <tr>
                <th>発注ログID</th>
                <th>登録者ID</th>
                <th>登録者名</th>
                <th>物品ID</th>
                <th>物品名</th>
                <th>カテゴリ</th>
                <th>発注数</th>
                <th>削除</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="order in orderList" :key="order.ItemId">
                <td>{{ order.OrderLogId }}</td>
                <td>{{ order.StaffId }}</td>
                <td>{{ order.StaffName }}</td>
                <td>{{ order.ItemId }}</td>
                <td>{{ order.ItemName }}</td>
                <td>{{ order.ItemKinds }}</td>
                <td>{{ order.OrderNum }}</td>
                <td>
                  <button @click="removeOrder(order.ItemId)">削除</button>
                </td>
              </tr>
            </tbody>
          </table>
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
const orderList = ref([])

onMounted(async () => {
  const res = await axios.get('http://localhost:5022/api/stock/logs')
  stockList.value = res.data
  // 必要に応じてorderListも初期化
})

function addToOrderList(item) {
  // ...（省略。さきほどのまま使ってOK）
}
function removeOrder(itemId) {
  orderList.value = orderList.value.filter(o => o.ItemId !== itemId)
}
function resetOrderList() {
  orderList.value = []
}
function submitOrders() {
  // ...（さきほどのまま使ってOK）
}
</script>

<style scoped>
body, html, #app {
  margin: 0;
  padding: 0;
  background: #460125;
  min-height: 100vh;
  width: 100vw;
  box-sizing: border-box;
}

.home-root {
  display: flex;
  min-height: 100vh;
  background: #460125;
  width: 100vw;
}
.main-area {
  display:flex;
  padding: 48px 40px 40px 40px;
  background: #f3ede6;
  min-width: 320px;
  margin: 0;
}

.main-title, .section-title {
  font-family: 'Yu Mincho', serif;
  font-size: 1.8rem;
  text-align: center;
  color: #c0c0c0;
  margin: 8px 0 22px 0;
  letter-spacing: 0.1em;
}

.stock-table-wrap, .order-table-wrap {
  width: 80%;
  max-width: 980px;
  margin: 0 auto 24px auto;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px #bb99aa33;
  min-height: 210px;
  overflow-x: auto;
  overflow-y: auto;
  padding-bottom: 12px;
  padding-top: 6px;
  
}

.table {
  width: 50%;
  max-width: 800px;
  margin: 0 auto 24px auto;
  background: #fff;
  border-radius: 8px;
  border-collapse: collapse;
  font-size: 1.05rem;
  margin-bottom: 12px;
  min-width: 900px;
}

.table thead tr {
  background: #1a1113;
  color: #fff;
}

.table th, .table td {
  padding: 18px 20px;   /* ←ここの数字を大きくすると余白が広くなる */
  border: 1px solid #b8a8ad;
  text-align: center;
  /* 追加例: */
  /* vertical-align: middle; */
}


.table td {
  background: #fff;
}

.order-btn-row {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 40px;
  margin-top: 32px;
  margin-bottom: 20px;
}

.order-btn-big {
  background: #eaf8ff;
  color: #486578;
  font-size: 2rem;
  padding: 14px 62px;
  border: 1.5px solid #b7d1e5;
  border-radius: 12px;
  cursor: pointer;
  transition: background 0.18s;
  z-index: 2;
}

.cancel-btn-big {
  background: #edd8ed;
  color: #94618e;
  font-size: 2rem;
  padding: 14px 62px;
  border: 1.5px solid #cbb4ce;
  border-radius: 12px;
  cursor: pointer;
  transition: background 0.18s;
  z-index: 2;
}

.order-btn-big:hover { background: #d2eefd; }
.cancel-btn-big:hover { background: #fbe5fa; }

@media (max-width: 1100px) {
  .main-area {
    padding: 24px 6vw 24px 6vw;
  }
  .table, .stock-table-wrap, .order-table-wrap {
    min-width: 630px;
    max-width: 100vw;
  }
}

@media (max-width: 700px) {
  .main-area,
  .sidebar {
    padding: 2vw 1vw;
  }
  .table {
    font-size: 0.93rem;
    min-width: 350px;
  }
  .order-btn-row {
    flex-direction: column;
    gap: 10px;
  }
  .order-btn-big, .cancel-btn-big {
    font-size: 1.15rem;
    padding: 8px 24px;
  }
}
</style>