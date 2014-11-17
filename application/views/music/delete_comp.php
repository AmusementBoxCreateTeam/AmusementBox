<?php $this->load->view('common/header'); ?>
<div class="container-fluid">
    <h2>音楽削除完了</h2>
    <?php echo validation_errors(); ?>
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">音楽削除完了</div>
        </div>
        <p class="text-center">
            音楽の削除が完了しました。<br />
            <a href="<?php echo base_url() . 'index.php/music' ?>">音楽一覧へ</a>
        </p>
    </div>
</div>
<?php
$this->load->view('common/footer');
