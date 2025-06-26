<template>
<div>
    <!-- ハンバーガーアイコン（スマホサイズで表示） -->
    <button class="hamburger" @click="isMenuOpen = !isMenuOpen">
        ☰
    </button>
    <!-- メニューのオーバーレイと中身（スマホ用） -->
    <div v-if="isMenuOpen" class="overlay" @click.self="isMenuOpen = false">
        <transition name="slide">
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
</transition>
</div>

<!-- PC用メニュー -->
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
</div>
</template>




<script setup>

import { useRoute, useRouter } from 'vue-router'
import { ref, onMounted, onUnmounted } from 'vue'


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


const isMenuOpen = ref(false)
const route = useRoute()
const router = useRouter()

const reloadIfSame = (path) => {
    if (route.path === path) {
        router.go(0)
    }
}

const isActive = (path) => route.path === path
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
    align-items: center;
    text-align: center;
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

.hamburger {
    display: none;
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
        background-color: rgba(128, 128, 128, 0.4);
        z-index: 1090;
    }

    .overlay .side-menu2 {
        transform: translateX(0);
    }

    .side-menu2 {
        position: fixed;
        top: 0;
        left: 0;
        width: 250px;
        height: 100vh;
        background-color: #fefaf6;
        padding: 30px;
        box-shadow: 2px 0 10px rgba(0, 0, 0, 0.2);
        z-index: 1101;
        transform: translateX(-100%);
        transition: transform 0.3s ease-in-out;
    }
    
    .slide-enter-active,
    .slide-leave-active {
        transition: transform 0.3s ease;
    }
    
    .slide-enter-from,
    .slide-leave-to {
        transform: translateX(-100%);
    }
    
    .side-menu2-wrapper {
        display: none;
    }
}

</style>
