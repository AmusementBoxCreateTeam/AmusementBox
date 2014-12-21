<?php $this->load->view('common/header'); ?>
<div class="container-fluid">
    <ul class="breadcrumb">
        <li>端末登録<span class="divider"></span></li>
        <li>端末登録確認<span class="divider"></span></li>
        <li class="active">端末登録完了</li>
    </ul>
    <h2>端末登録完了</h2>
    <?php echo validation_errors(); ?>
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">端末登録完了</div>
        </div>
        <p class="text-center">
            端末登録が完了しました。<br />
            <a href="<?php echo base_url() . 'index.php/box' ?>">端末一覧へ</a>
        </p>
    </div>
</div>
<?php
$this->load->view('common/footer');
