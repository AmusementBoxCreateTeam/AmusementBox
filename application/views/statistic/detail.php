<?php $this->load->view('common/header'); ?>
<div class="container-fluid">
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">音楽ランキング詳細</div>
        </div>
        <?php/*
        $data = '';
        $data['man'] = $man;
        $data['woman'] = $woman;
        $this->load->view('statistic/pie_graph', $data);
         * 
         */
        ?>
        <IMG src="<?php echo base_url().'html/img/pie_graph.php' ?>" width="200" height="200">
    </div>
</div>
<?php $this->load->view('common/footer'); ?>