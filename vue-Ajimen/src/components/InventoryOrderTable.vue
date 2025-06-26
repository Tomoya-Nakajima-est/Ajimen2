<template>
  <div>
    <!-- 在庫表 -->
    <section>
      <h2 class="section-title">－在庫表－</h2>
      <div class="stock-table-wrap">
        <div class="search-row">
          <input class="search-input" placeholder="検索の際はここに入力" v-model="searchKeyword"/>
          <button class="search-btn" @click="searchStock">検索</button>
          <button class="search-btn" @click="searchKeyword=''; searchStock();">クリア</button>
        </div>
        <table class="table" id="stockTable">
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
                style="width: 60px; text-align: right;"
              />
              </td>
              <td>{{ stock.stockMinNum }}</td>
              <td>{{ stock.itemId }}</td>
              <td>{{ stock.itemName }}</td>
              <td>{{ stock.itemKinds }}</td>
              <td>
                <button @click="addToOrderList(stock)">発注</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
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
            <tr v-for="(order, idx) in orderList" :key="order.ItemId">
              <td>{{ idx + 1 }}</td>
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

const stockList = ref([]) // 在庫データ
const filteredStockList = ref([])　// 表示用（検索後データ）
const searchKeyword = ref('') //検索ワード
const orderList = ref([])

onMounted(async () => {
  const res = await axios.get('http://localhost:5022/api/stock/logs')
  stockList.value = res.data
  filteredStockList.value = res.data //初回は全件表示
  // 必要に応じてorderListも初期化
})

// 検索実行メソッド
function searchStock() {
  const keyword = searchKeyword.value.trim().toLowerCase();
  if (!keyword) {
    filteredStockList.value = [...stockList.value]; // 空なら全件
    return;
  }
  filteredStockList.value = stockList.value.filter(stock =>
    String(stock.itemId).includes(keyword) ||
    String(stock.itemName).toLowerCase().includes(keyword)
  );
}

// 在庫数保存メソッド
async function updateStockNum(stock) {
  try {
    await axios.post('http://localhost:5022/api/stock/update', {
      stockLogId: stock.stockLogId,
      stockNum: stock.stockNum,
      // 必要に応じ他のプロパティも
    });
    // 成功時の処理
  } catch (e) {
    alert('更新に失敗しました');
  }
}

//　発注数入力メソッド
function addToOrderList(item) {
  const orderNum = prompt("発注数を入力してください", 1)
  if (orderNum !== null && !isNaN(Number(orderNum))) {
    const staffId = localStorage.getItem('userId')
    const staffName = localStorage.getItem('userFullName')
    const existing = orderList.value.find(o => o.ItemId === item.itemId)
    const orderData = {
      OrderLogId: 0,
      StaffId: staffId,
      StaffName: staffName,   // ←これで氏名が必ず入る
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
// 発注データ送信API例
async function submitOrders() {
  if (orderList.value.length === 0) return
  try {
    const res = await axios.post('http://localhost:5022/api/orderlog/submitandmail', orderList.value)
    // 成功時のハンドリング
    if (res.status === 200) {
      alert("発注内容を保存し、メール送信に成功しました。\n" + (res.data || ""));
      orderList.value = []
    } else {
      alert("サーバーがエラー応答を返しました: " + (res.data || "詳細不明"))
    }
  } catch (e) {
    // エラー時のハンドリング
    console.log(JSON.stringify(orderList.value));
    let msg = "保存またはメール送信時にエラーが発生しました。"
    if (e.response && e.response.data) {
      msg += "\n詳細: " + e.response.data
    }
    alert(msg)
  }
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
  overflow-y: auto;
  height: 100vh; /* または100% */
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
  overflow-y: auto;
  height: 100vh; /* または100% */
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
  width: 100%;
  max-width: 100vw;
  min-width: 0;
  margin: 0 auto 10px auto;      /* 下余白を減らす */
  padding: 4px 0;                /* パディングを小さく */
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px #bb99aa33;
  min-height: 50px;
  overflow-x: auto;
}

.table {
  width: 100%;
  min-width: 360px;              /* これより小さい画面は横スクロール */
  max-width: 100%;
  font-size: 0.88rem;            /* やや小さめ */
  border-radius: 8px;
  border-collapse: collapse;
}

.table thead tr {
  background: #3a2e4e;
  color: #fff;
}

.low-stock > td {
  background-color: #be6f4a !important;
  color: #fff;
}

.table th, .table td {
  padding: 3px 4px;   /* ←ここの数字を大きくすると余白が広くなる */
  border: 1px solid #b8a8ad;
  text-align: center;
  /* 追加例: */
  /* vertical-align: middle; */
}

.table td {
  background: #fff;
}

.input-mini, .input-date-mini {
  width: 38px;
  font-size: 0.7rem;
  padding: 1px 2px;
  height: 18px;
  box-sizing: border-box;
}
.input-date-mini { width: 78px; }

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

.sidebar {
  min-width: 100px;
  font-size: 0.1rem;
  padding: 6px 4px;
}

.order-btn-big:hover { background: #d2eefd; }
.cancel-btn-big:hover { background: #fbe5fa; }

@media (max-width: 1000px) {
  .table, .stock-table-wrap, .order-table-wrap {
    max-width: 100vw;
    min-width: 0;
  }
}

@media (max-width: 700px) {
  .main-area,
  .sidebar {
    padding: 2vw 1vw;
  }
  .table {
    font-size: 0.93rem;
    min-width: 240px;
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

button,
.btn-mini,
.order-btn-big,
.cancel-btn-big {
  border-radius: 999px !important; /* かなり丸い、pill型 */
}
</style>