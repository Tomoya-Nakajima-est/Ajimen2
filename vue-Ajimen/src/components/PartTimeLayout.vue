
<template>
<div class="side-menu2-wrapper">
<div class="side-menu2">
    <h1>&lt;MENDACO&gt;</h1>
    <h2>-MENU-</h2>
    <nav>
        <router-link
        to="/parttime"
        class="menu-button"
        :class="{ active: isActive('/parttime') }"
        @click="reloadIfSame('/parttime')"
        >
        HOME
    </router-link>
    <router-link
        to="/parttimeshift"
        class="menu-button"
        :class="{ active: isActive('/parttimeshift') }"
        >
        シフト表
    </router-link>
</nav>
</div>
</div>
</template>

<script setup>
import { useRoute ,useRouter} from 'vue-router'

const route = useRoute()
const router = useRouter()

const reloadIfSame = (path) => {
    if (route.path === path) {
        router.go(0) // ← ページをリロード
        }
}

const isActive = (path) => {
    return route.path === path
}
</script>

<style scoped>
.side-menu2-wrapper {
    width: 210px;
    display: flex;
    justify-content: center; 
    align-items: center;
    height: 99vh;
    background-color: transparent; /* 背景は親に任せる */
    position: fixed;
    top: -40px;
    left: 100px;
    z-index: 1000;
    /* border-right: 2px solid #ccc; */
    pointer-events: none; /* 背景の線を見えるように */
}

.side-menu2 {
    background-color: #fefaf6;
    padding: 20px 20px 40px; 
    box-shadow: none;/*白線*/
    border-radius: 8px;
    width: 200px;
    font-family: 'Georgia', serif;
    color: #203744;
    pointer-events: auto; /* メニューは操作可能に */
    height: 80vh;
    display: flex;
    flex-direction: column;
    justify-content: center;
    gap: 30px;
    position: relative;
    z-index: 1001;

}

/* 背景のクロス線をbodyや親要素に追加 */
body::before,
body::after {
    content: '';
    position: absolute;
    background-color: black;
    z-index: 0;
}

/* 横線 */
body::before {
    bottom: 60px;
    left: 0;
    width: 100%;
    height: 50px;
}

/* 縦線 */
body::after {
    bottom: 0;
    left: 120px;
    width: 50px;
    height: 100%;
}

h1 {
    font-size: 24px;
    margin-bottom: 10px;
    color: #460125;
}

h2 {
    font-size: 20px;
    text-align: center;
    margin-bottom: 20px;
}

nav {
    display: flex;
    flex-direction: column;
}

.menu-button {
    display: block;
    width: 100%;
    margin-bottom: 10px;
    padding: 30px;
    font-size: 20px;
    background-color: #f1ebe5;
    text-align: center;
    text-decoration: none;
    color: #203744;
    transition: background-color 0.3s;
    gap: 20px;
}

.menu-button:hover {
    background-color: #93ca76;
}

.menu-button.active {
    background-color: #f8e58c;
    color: #3a525b;
    font-weight: bold;
    pointer-events: auto; 
}

.menu-button.active:hover{
    background-color: #5d92a5; 
    color: #f8e58c;
}

/* スマホ対応 */
@media (max-width: 768px) {
    .side-menu-wrapper {
        width: 100%;
        height: auto;
        position: relative;
        display: flex;
        flex-direction: column;
        align-items: center;
        padding: 20px 0;
    }
    .side-menu{
        width: 90;
    }

    .menu-button {
        font-size: 16px;
        padding: 12px;
    }
}
</style>
