<?php $this->load->view('common/header'); ?>
<div class="container-fluid">
    <ul class="breadcrumb">
        <li>音楽<?php echo $method ?><span class="divider"></span></li>
        <li>音楽<?php echo $method ?>確認<span class="divider"></span></li>
        <li class="active">音楽<?php echo $method ?>完了</li>
    </ul>
    <h2>音楽<?php echo $method ?>完了</h2>
    <?php echo validation_errors(); ?>
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">音楽<?php echo $method ?>完了</div>
        </div>
        <p class="text-center">
            音楽の<?php echo $method ?>が完了しました。<br />
            <a href="<?php echo base_url() . 'index.php/music' ?>">音楽一覧へ</a>
        </p>
    </div>
</div>
<?php
$this->load->view('common/footer');
