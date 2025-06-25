<template>
  <!-- ここにホームと繋がるサイドバーを後入れする。 -->
  <div class="home-root">
    <!-- サイドバー（省略。上記デザイン参照） -->
    <aside class="sidebar">
      <div class="logo">&lt;AJIQ&gt;</div>
      <div class="menu-title">－MENU－</div>
      <button class="menu-btn">HOME</button>
      <button class="menu-btn">シフト表</button>
      <button class="menu-btn active">発注・在庫</button>
    </aside>

    <main class="main-area">
      <h2 class="main-title">－在庫表－</h2>
      <table class="table">
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
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'

const stockList = ref([])
const allStockList = ref([])
const orderList = ref([])
const searchKeyword = ref('')

// 在庫データ取得
onMounted(async () => {
  const res = await axios.get('http://localhost:5022/api/stock/logs')
  stockList.value = res.data
  allStockList.value = res.data // 検索リセット用
})

// 例：発注リストに商品を追加
function addToOrderList(item) {
  const orderNum = prompt("発注数を入力してください", 1)
  if (orderNum !== null && !isNaN(Number(orderNum))) {
    // すでに同じ商品がリストにある場合は更新
    const existing = orderList.value.find(o => o.ItemId === item.itemID)
    const orderData = {
      OrderLogId: 0, // 新規作成なので0またはnull
      StaffId: "0001", // 実際はログインユーザーから取得
      StaffName: "発注太郎", // 実際はログインユーザーから取得
      OrderNum: Number(orderNum),
      ItemId: item.itemID,
      ItemName: item.itemName,
      ItemKinds: item.itemKinds || "未分類",
      // OrderDayとuseOrderLogはAPIでセットされるので未指定
    }
    if (existing) {
      Object.assign(existing, orderData)
    } else {
      orderList.value.push(orderData)
    }
  }
}

function removeOrder(itemId) {
  orderList.value = orderList.value.filter(o => o.itemId !== itemId)
}

function resetOrderList() {
  orderList.value = []
}

function searchStock() {
  if (!searchKeyword.value) {
    stockList.value = allStockList.value
  } else {
    stockList.value = allStockList.value.filter(
      s => s.itemName && s.itemName.includes(searchKeyword.value)
    )
  }
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
  background: #75022d;
  min-height: 100vh;
  width: 100vw;
  box-sizing: border-box;
}

.home-root {
  display: flex;
  min-height: 100vh;
  background: #75022d;
  width: 100vw;
}

.sidebar {
  background: #efe8e3;
  min-width: 200px;
  max-width: 240px;
  padding: 40px 0 0 0;
  text-align: center;
  border-radius: 0 0 0 0;
  box-shadow: 0 0 10px #bb99aa33;
  display: flex;
  flex-direction: column;
  align-items: center;
  height: 100vh;
  border-right: 8px solid #6b2236;
  margin: 0;
}

.main-area {
  flex: 1;
  padding: 48px 40px 40px 40px;
  background: #f3ede6;
  min-width: 320px;
  margin: 0;
}

.main-title, .section-title {
  font-family: 'Yu Mincho', serif;
  font-size: 2.1rem;
  text-align: center;
  color: #555;
  margin: 8px 0 22px 0;
  letter-spacing: 0.12em;
}

.stock-table-wrap, .order-table-wrap {
  width: 100%;
  max-width: 980px;
  margin: 0 auto 24px auto;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px #bb99aa33;
  min-height: 210px;
  overflow-x: auto;
  padding-bottom: 12px;
  padding-top: 6px;
}

.table {
  width: 100%;
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

