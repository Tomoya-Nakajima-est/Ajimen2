<template>
  <div class="login-container">
    <div class="login-box">
      <h1>AJIQ / MENDACO LOGIN</h1>

      <label for="id">ID</label>
      <input v-model="id" id="id" type="text" placeholder="ここに入力してください" />

      <label for="password">パスワード/pass word</label>
      <input v-model="password" id="password" type="password" placeholder="ここに入力してください" />

      <button @click="handleLogin">Login</button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { login } from '@/services/authService'

const id = ref('')
const password = ref('')
const router = useRouter()

const handleLogin = async () => {
  try {
    const res = await login(id.value, password.value)
    localStorage.setItem('userId', res.userId)
    localStorage.setItem('userFullName', res.fullName)   // ←追加 0626
    localStorage.setItem('userRole', res.role) // ←追加 0626
    if (res.role === '正社員' || res.role === '経営者') {
      router.push('/employee')
    } else if (res.role === 'アルバイト') {
      router.push('/parttime')
    } else {
      alert('不明な権限です')
    }
  } catch (err) {
    alert('ログイン失敗')
  }
}
</script>

<style scoped>
.login-container {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: #460125;
  overflow: hidden;
  z-index: 0;
}
/*クロス線の追加*/ 
.login-container::before,
.login-container::after{
  content:'';
  position: absolute;
  background-color: black;
  z-index: 1;
}
/*横線*/
.login-container::before{
  bottom: 60px;
  left: 0;
  width: 100%;
  height: 50px;
  transform: rotate(0deg);
}

/*縦線*/
.login-container::after{
  bottom: 0px;
  left: 120px;
  width: 50px;
  height: 100%;
  transform: rotate(0deg);
}

.login-box {
  background-color: #fefaf6;
  width: 400px;
  padding: 40px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  border: 1px solid #ccc;
  text-align: center;
  font-family: 'Georgia', serif;
  z-index: 2;
}

h1 {
  font-size: 22px;
  margin-bottom: 30px;
}

.form-group {
  margin-bottom: 20px;
  text-align: left;
}

label {
  font-weight: bold;
  display: block;
  margin-bottom: 5px;
}

input {
  width: 100%;
  padding: 8px;
  font-size: 14px;
  box-sizing: border-box;
  border: 1px solid #999;
}

button {
  margin-top: 20px;
  padding: 10px 30px;
  font-size: 16px;
  background-color: #e3e3e3;
  border: none;
  cursor: pointer;
}

button:hover {
  background-color: #ccc;
}
</style>