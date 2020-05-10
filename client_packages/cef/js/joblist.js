var jobselector = new Vue({
    el: ".joblist",
    data: {
        active: false,
        jobid: -1,
        level: 1,
        list: 
        [
            {class: "svar", name: "Сварщик", level: 0, jobid: 9},
            {class: "spais", name: "Спайс", level: 0, jobid: 10},
            {class: "electro", name: "Электрик", level: 0, jobid: 1},
            {class: "taxi", name: "Таксист", level: 1, jobid: 3},
            {class: "bus", name: "Водитель автобуса", level: 1, jobid: 4},
            {class: "mechanic", name: "Автомеханик", level: 1, jobid: 8},
            {class: "truck", name: "Дальнобойщик", level: 1, jobid: 6},
            {class: "inkos", name: "Инкассатор", level: 1, jobid: 7},
            {class: "gazon", name: "Скоро", level: 20, jobid: 5},
            {class: "pochta", name: "Скоро", level: 20, jobid: 2},
        ],
    },
    methods: {
        closeJobMenu: function() {
            mp.trigger("closeJobMenu");
        },
        show: function (level, currentjob) {
            this.level = level;
            this.jobid = currentjob;
            this.active = true;
        },
        hide: function () {
            this.active = false;
        },
        selectJob: function(jobid) {
            mp.trigger("selectJob", jobid);
        }
    }
})