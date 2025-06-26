
<template>
<div>
<!-- ハンバーガーアイコン（スマホサイズで表示） -->
<button class="hamburger" @click="isMenuOpen = !isMenuOpen">
    ☰
</button>

<!-- メニューのオーバーレイと中身 -->
<div v-if="isMenuOpen" class="overlay" @click.self="isMenuOpen = false">
    <transition name="slide">
        <div class="side-menu1">
            <!--<button class="close-button" @click="isMenuOpen = false"></button>-->
            <h1>&lt;AJIQ&gt;</h1>
            <h2>-MENU-</h2>
            <nav>
                <router-link
                to="/employee"
                class="menu-button"
                :class="{ active: isActive('/employee') }"
                @click="reloadIfSame('/employee')"
                >
                HOME
            </router-link>
            <router-link
            to="/employeeshift"
            class="menu-button"
            :class="{ active: isActive('/employeeshift') }"
            >
            シフト表
            </router-link>
            <router-link
            to="/employeeorder"
            class="menu-button"
            :class="{ active: isActive('/employeeorder') }"
            >
            発注・在庫
            </router-link>
            </nav>
        </div>
    </transition>
</div>

<div class="side-menu1-wrapper">
    <div class="side-menu1">
    <h1>&lt;AJIQ&gt;</h1>
    <h2>-MENU-</h2>
    <nav>
        <router-link
        to="/employee"
        class="menu-button"
        :class="{ active: isActive('/employee') }"
        @click="reloadIfSame('/employee')"
        >
        HOME
    </router-link>
    <router-link
    to="/employeeshift"
    class="menu-button"
    :class="{ active: isActive('/employeeshift') }"
    >
    シフト表
</router-link>
<router-link
to="/employeeorder"
class="menu-button"
:class="{ active: isActive('/employeeorder') }">
発注・在庫
</router-link>
</nav>
</div>
</div>
</div>
</template>

<script setup>
import { useRoute ,useRouter} from 'vue-router'
import { ref , onMounted, onUnmounted} from 'vue'
const isMenuOpen = ref(false)

const handleResize = () => {
    if (window.innerWidth > 768) {
        isMenuOpen.value = false
    }
}

onMounted(() => {
    window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
    window.removeEventListener('resize', handleResize)
})

const route = useRoute()
const router = useRouter()


const reloadIfSame = (path) => {
    if (route.path === path) {
        router.go(0) // ← ページをリロード
        }
}

const isActive = (path) => route.path === path
</script>

<style scoped>

.side-menu1-wrapper {
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

.side-menu1 {
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
    max-height: 100vh;
    overflow-y:auto ;
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

.hamburger {
    display: none;
}



h1 {
    font-size: 34px;
    margin-top: 0; 
    color: #460125;
    text-align: center;
}

h2 {
    font-size: 30px;
    text-align: center;
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
    color: #460125;
    font-weight: bold;
    pointer-events: auto; 

}


.menu-button.active:hover {
    background-color: #5e1a35; /* ← えんじ色を少し明るく */
    color: #f8e58c;
}


/* スマホ対応 */

@media (max-width: 768px) {
    .hamburger {
        display: block;
        position: fixed;
        top: 10px;
        left: 10px;
        font-size: 30px;
        z-index: 1100;
        background: none;
        border: none;
        cursor: pointer;
    }
    .overlay {
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw;
        height: 100vh;
        background-color: rgba(128, 128, 128, 0.4); /* 薄いグレーの幕 */
        z-index: 1090;
    }

    .side-menu1 {
        position: fixed;
        top: 0;
        left: 0;
        width: 250px;height: 100vh;
        background-color: #fefaf6;
        padding: 30px;
        box-shadow: 2px 0 10px rgba(0,0,0,0.2);
        z-index: 1101;
        transform: translateX(-100%);
        transition: transform 0.3s ease-in-out;
    }

    .overlay .side-menu1 {
        transform: translateX(0)
    }

    .slide-enter-active,
    .slide-leave-active {
        transition: transform 0.3s ease;
    }

    .slide-enter-from,
    .slide-leave-to {
        transform: translateX(-100%);
    }

 /* PCではサイドメニュー非表示 */
.side-menu1-wrapper {
    display: none;
}
}

</style>
